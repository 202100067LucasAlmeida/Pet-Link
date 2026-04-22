using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using PetLink.Services;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace PetLink.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public MessagesController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /Messages/Index/5?animalId=12
        // Recebe o ID do destinatário (id) e opcionalmente o ID do animal (animalId)
        public async Task<IActionResult> Index(int? id, int? animalId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();
            int currentUserId = int.Parse(userIdClaim);

            // 1. Ir buscar as mensagens, INCLUINDO o AnimalListing para saber o nome do pet
            var allMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.AnimalListing)
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .ToListAsync();

            // 2. Agrupar por PESSOA + ANIMAL
            var conversations = allMessages
                .GroupBy(m => new
                {
                    OtherUserId = m.SenderId == currentUserId ? m.ReceiverId : m.SenderId,
                    AnimalId = m.AnimalListingId
                })
                .Select(g =>
                {
                    // Ordenar as mensagens deste grupo para apanhar a mais recente
                    var latestMsgInGroup = g.OrderByDescending(m => m.Timestamp).First();
                    var otherUser = latestMsgInGroup.SenderId == currentUserId ? latestMsgInGroup.Receiver : latestMsgInGroup.Sender;

                    return new ConversationSummary
                    {
                        OtherUserId = g.Key.OtherUserId,
                        OtherUserName = otherUser.Name,
                        OtherUserImagePath = otherUser.ProfilePicture,
                        AnimalListingId = g.Key.AnimalId,
                        AnimalName = latestMsgInGroup.AnimalListing?.Name, // Nome do pet associado
                        LastMessagePreview = latestMsgInGroup.Content,
                        LastMessageTimestamp = latestMsgInGroup.Timestamp,
                        // É a conversa ativa se bater certo com o User E com o Animal
                        IsActive = (id.HasValue && g.Key.OtherUserId == id.Value && g.Key.AnimalId == animalId),
                        UnreadCount = g.Count(m => m.ReceiverId == currentUserId && !m.IsRead),
                        IsOnline = true
                    };
                })
                .OrderByDescending(c => c.LastMessageTimestamp)
                .ToList();

            var viewModel = new MessagesViewModel { Conversations = conversations };

            // 3. Se uma conversa estiver selecionada (Ativa)
            if (id.HasValue)
            {
                var otherUser = await _context.Users.FindAsync(id.Value);
                var animal = animalId.HasValue ? await _context.AnimalListings.FindAsync(animalId.Value) : null;

                if (otherUser != null)
                {
                    viewModel.ActiveConversation = new ConversationDetail
                    {
                        OtherUserId = id.Value,
                        OtherUserName = otherUser.Name,
                        OtherUserImagePath = otherUser.ProfilePicture,
                        AnimalListingId = animalId,
                        AnimalName = animal?.Name,
                        // Filtrar as mensagens para esta pessoa E para este animal
                        Messages = allMessages
                            .Where(m => ((m.SenderId == currentUserId && m.ReceiverId == id.Value) ||
                                         (m.SenderId == id.Value && m.ReceiverId == currentUserId)) &&
                                        m.AnimalListingId == animalId)
                            .OrderBy(m => m.Timestamp)
                            .ToList()
                    };
                }
            }

            return View(viewModel);
        }

        // POST: /Messages/SendMessage
        [HttpPost]
        [Authorize]
        // Recebe também o animalId vindo do Javascript
        public async Task<IActionResult> SendMessage(int receiverId, int? animalId, string content)
        {
            Console.WriteLine("=== ENTROU NO SENDMESSAGE ===");
            Console.WriteLine($"ReceiverId: {receiverId}, AnimalId: {animalId}");
            Console.WriteLine($"Content: {content}");

            var senderIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(senderIdClaim) || string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Invalid sender or empty content" });
            }

            int senderId = int.Parse(senderIdClaim);

            if (senderId == receiverId)
            {
                return Json(new { success = false, message = "You cannot message yourself." });
            }

            var sender = await _context.Users.FindAsync(senderId);
            var receiver = await _context.Users.FindAsync(receiverId);

            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                AnimalListingId = animalId, // Gravar o contexto do animal!
                Content = content.Trim(),
                Timestamp = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // ========== ENVIAR NOTIFICAÇÃO POR EMAIL ==========
            Console.WriteLine("=== TENTANDO ENVIAR EMAIL ===");
            if (receiver != null && !string.IsNullOrEmpty(receiver.Email))
            {
                try
                {
                    Console.WriteLine($"A enviar email para: {receiver.Email}");
                    await _emailService.SendNewMessageNotificationAsync(
                        receiver.Email,
                        receiver.Name,
                        sender?.Name ?? "Someone",
                        content.Length > 100 ? content.Substring(0, 100) + "..." : content
                    );
                    Console.WriteLine("✅ Email enviado com sucesso!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao enviar email: {ex.Message}");
                }
            }
            // ========== FIM DA NOTIFICAÇÃO ==========

            return Json(new { success = true, messageId = newMessage.Id, timestamp = newMessage.Timestamp.ToString("HH:mm") });
        }
    }
}