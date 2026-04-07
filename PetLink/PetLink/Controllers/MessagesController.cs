using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PetLink.Services;
using System.Threading.Tasks;

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

        // GET: /Messages/Index ou /Messages/Index/5
        public async Task<IActionResult> Index(int? id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();
            int currentUserId = int.Parse(userIdClaim);

            var allMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            // Criar a lista de conversas com IMAGEM
            var conversations = allMessages
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Select(g =>
                {
                    var firstMsg = g.First();
                    var otherUser = firstMsg.SenderId == currentUserId ? firstMsg.Receiver : firstMsg.Sender;
                    return new ConversationSummary
                    {
                        OtherUserId = g.Key,
                        OtherUserName = otherUser.Name,
                        OtherUserImagePath = otherUser.ProfilePicture,
                        LastMessagePreview = firstMsg.Content,
                        LastMessageTimestamp = firstMsg.Timestamp,
                        IsActive = (id.HasValue && g.Key == id.Value),
                        IsOnline = true
                    };
                })
                .ToList();

            var viewModel = new MessagesViewModel { Conversations = conversations };

            if (id.HasValue)
            {
                var otherUser = await _context.Users.FindAsync(id.Value);
                if (otherUser != null)
                {
                    viewModel.ActiveConversation = new ConversationDetail
                    {
                        OtherUserId = id.Value,
                        OtherUserName = otherUser.Name,
                        OtherUserImagePath = otherUser.ProfilePicture,
                        Messages = allMessages
                            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == id.Value) ||
                                        (m.SenderId == id.Value && m.ReceiverId == currentUserId))
                            .OrderBy(m => m.Timestamp)
                            .ToList()
                    };
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage(int receiverId, string content)
        {
            Console.WriteLine("=== ENTROU NO SENDMESSAGE ===");
            Console.WriteLine($"ReceiverId: {receiverId}");
            Console.WriteLine($"Content: {content}");
            // Obtém o ID do utilizador logado 
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

            // Buscar informações do remetente e destinatário
            var sender = await _context.Users.FindAsync(senderId);
            var receiver = await _context.Users.FindAsync(receiverId);

            // Cria o objeto da mensagem
            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content.Trim(),
                Timestamp = DateTime.Now,
                IsRead = false
            };

            // Guarda na Base de Dados
            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // ========== ENVIAR NOTIFICAÇÃO POR EMAIL ==========
            Console.WriteLine("=== TENTANDO ENVIAR EMAIL ===");
            Console.WriteLine($"Destinatário: {receiver?.Email}");
            Console.WriteLine($"Destinatário existe? {receiver != null}");
            // Enviar email apenas se o destinatário existe e tem email válido
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
            Console.WriteLine($"Detalhes: {ex.StackTrace}");
        }
    }
    else
    {
        Console.WriteLine("❌ Destinatário inválido ou sem email");
    }
            // ========== FIM DA NOTIFICAÇÃO ==========

            return Json(new { success = true, messageId = newMessage.Id, timestamp = newMessage.Timestamp.ToString("HH:mm") });
        }
    }
}