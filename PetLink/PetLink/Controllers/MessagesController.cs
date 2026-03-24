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
            // Obter o ID do utilizador logado com segurança
            var userIdClaim = User.FindFirst("UserId")?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();
            int currentUserId = int.Parse(userIdClaim);

            // Procurar todas as mensagens onde o user participa
            var allMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            // 3Criar a lista de conversas 
            var conversations = allMessages
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Select(g => new ConversationSummary
                {
                    OtherUserId = g.Key,
                    OtherUserName = g.First().SenderId == currentUserId ? g.First().Receiver.Name : g.First().Sender.Name,
                    LastMessagePreview = g.First().Content,
                    LastMessageTimestamp = g.First().Timestamp,
                    IsActive = (id.HasValue && g.Key == id.Value),
                    IsOnline = true
                })
                .ToList();

            var viewModel = new MessagesViewModel { Conversations = conversations };

            // Se houver um ID na URL, os detalhes dessa conversa são carregados
            if (id.HasValue)
            {
                var otherUser = await _context.Users.FindAsync(id.Value);
                if (otherUser != null)
                {
                    // 1. Prepara a janela da direita (mensagens vazias se for novo)
                    viewModel.ActiveConversation = new ConversationDetail
                    {
                        OtherUserId = id.Value,
                        OtherUserName = otherUser.Name,
                        Messages = allMessages
                            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == id.Value) ||
                                        (m.SenderId == id.Value && m.ReceiverId == currentUserId))
                            .OrderBy(m => m.Timestamp)
                            .ToList()
                    };

                    // 2. Se for um chat novo, adiciona-o ao topo da barra lateral!
                    if (!conversations.Any(c => c.OtherUserId == id.Value))
                    {
                        conversations.Insert(0, new ConversationSummary
                        {
                            OtherUserId = id.Value,
                            OtherUserName = otherUser.Name,
                            LastMessagePreview = "Start a conversation...", // Mensagem de incentivo
                            LastMessageTimestamp = DateTime.Now,
                            IsActive = true,
                            IsOnline = true
                        });
                    }
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