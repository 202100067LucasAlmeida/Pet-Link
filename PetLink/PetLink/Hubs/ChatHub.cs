using Microsoft.AspNetCore.SignalR;
using PetLink.Data;
using PetLink.Models;

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
            var senderIdClaim = Context.User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(senderIdClaim)) return;

            int senderId = int.Parse(senderIdClaim);

            // Grava na Base de Dados 
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Envia em Tempo Real 
            // Enviamos para o Remetente e para o Destinatário
            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, content, message.Timestamp.ToString("HH:mm"));
            await Clients.User(senderId.ToString()).SendAsync("ReceiveMessage", senderId, content, message.Timestamp.ToString("HH:mm"));
        }
    }
}