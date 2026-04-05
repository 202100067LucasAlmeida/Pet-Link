using Microsoft.AspNetCore.SignalR;
using PetLink.Data;
using PetLink.Models;
using Microsoft.EntityFrameworkCore;

namespace PetLink.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendChatMessage(int receiverId, string content)
        {
            // 1. Validar utilizador
            var senderIdClaim = Context.User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(senderIdClaim) || string.IsNullOrWhiteSpace(content)) return;

            int senderId = int.Parse(senderIdClaim);

            if (senderId == receiverId) return;
            try
            {
                // 2. Criar e Gravar a Mensagem na BD
                var message = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Content = content.Trim(),
                    Timestamp = DateTime.Now,
                    IsRead = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync(); // Se isto falhar, o código abaixo não corre

                // 3. Identificar o Grupo (Sala de chat privada entre os dois)
                string groupName = GetGroupName(senderId, receiverId);

                // 4. Notificar TODOS no grupo (Sender e Receiver)
                // O SignalR enviará para todos os dispositivos ligados destes dois users nessa sala
                await Clients.Group(groupName).SendAsync("ReceiveMessage",
                    senderId,
                    message.Content,
                    message.Timestamp.ToString("HH:mm"));
            }
            catch (Exception ex)
            {
                // Log do erro para saberes porque não gravou
                Console.WriteLine($"[SignalR Error]: {ex.Message}");
                throw;
            }
        }

        public async Task JoinChat(int userId1, int userId2)
        {
            string groupName = GetGroupName(userId1, userId2);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"[SignalR]: User {Context.ConnectionId} joined {groupName}");
        }

        private string GetGroupName(int id1, int id2)
        {
            // Garante que o nome do grupo é sempre igual (ex: 3_5) independentemente de quem inicia
            var list = new List<int> { id1, id2 };
            list.Sort();
            return $"chat_{list[0]}_{list[1]}";
        }
    }
}