using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.ViewModels;
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
            // 1. Obter o ID do utilizador logado com segurança
            var userIdClaim = User.FindFirst("UserId")?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();
            int currentUserId = int.Parse(userIdClaim);

            // 2. Procurar todas as mensagens onde o user participa
            var allMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            // 3. Criar a lista de conversas (Coluna da Esquerda)
            // Agrupamos por "a outra pessoa" (quem não sou eu)
            var conversations = allMessages
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Select(g => new ConversationSummary
                {
                    OtherUserId = g.Key,
                    // Vamos buscar o nome da outra pessoa
                    OtherUserName = g.First().SenderId == currentUserId ? g.First().Receiver.Name : g.First().Sender.Name,
                    LastMessagePreview = g.First().Content,
                    LastMessageTimestamp = g.First().Timestamp,
                    IsActive = (id.HasValue && g.Key == id.Value),
                    IsOnline = true // Hardcoded para o mockup, podes evoluir depois
                })
                .ToList();

            var viewModel = new MessagesViewModel { Conversations = conversations };

            // 4. Se houver um ID na URL, carregamos os detalhes dessa conversa (Coluna da Direita)
            if (id.HasValue)
            {
                var otherUser = await _context.Users.FindAsync(id.Value);
                if (otherUser != null)
                {
                    viewModel.ActiveConversation = new ConversationDetail
                    {
                        OtherUserId = id.Value,
                        OtherUserName = otherUser.Name,
                        Messages = allMessages
                            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == id.Value) ||
                                        (m.SenderId == id.Value && m.ReceiverId == currentUserId))
                            .OrderBy(m => m.Timestamp) // Ordem cronológica para o chat
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
            // 1. Obtém o ID do utilizador logado (quem está a enviar)
            var senderIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(senderIdClaim) || string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Index", new { id = receiverId });
            }

            int senderId = int.Parse(senderIdClaim);

            // 2. Cria o objeto da mensagem
            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content.Trim(),
                Timestamp = DateTime.Now,
                IsRead = false
            };

            // 3. Guarda na Base de Dados
            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // 4. Redireciona de volta para a conversa para mostrar a nova mensagem
            // Se estivesses a usar SignalR, aqui chamarias o Hub. 
            // Por agora, o refresh da página fará o trabalho!
            return RedirectToAction("Index", new { id = receiverId });
        }
    }
}