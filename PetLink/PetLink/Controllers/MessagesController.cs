using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PetLink.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
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
                        OtherUserImagePath = otherUser.ProfilePicture, // <--- BUSCA A FOTO
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
                        OtherUserImagePath = otherUser.ProfilePicture, // <--- BUSCA A FOTO
                        Messages = allMessages
                            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == id.Value) ||
                                        (m.SenderId == id.Value && m.ReceiverId == currentUserId))
                            .OrderBy(m => m.Timestamp)
                            .ToList()
                    };

                    // Lógica de inserir chat novo no topo se não existir...
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage(int receiverId, string content)
        {
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

            return Json(new { success = true, messageId = newMessage.Id, timestamp = newMessage.Timestamp.ToString("HH:mm") });
        }
    }
}