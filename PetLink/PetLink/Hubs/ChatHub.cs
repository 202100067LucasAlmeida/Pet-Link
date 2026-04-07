using Microsoft.AspNetCore.SignalR;
using PetLink.Data;
using PetLink.Models;
using Microsoft.EntityFrameworkCore;
using PetLink.Services;
using Microsoft.Extensions.DependencyInjection;


namespace PetLink.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
         private readonly IServiceScopeFactory _scopeFactory; 

        public ChatHub(ApplicationDbContext context, IEmailService emailService, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _emailService = emailService;
            _scopeFactory = scopeFactory;
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

                // ========== ENVIAR NOTIFICAÇÃO POR EMAIL ==========
                 _ = Task.Run(async () =>
                {
                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                            
                            var sender = await dbContext.Users.FindAsync(senderId);
                            var receiver = await dbContext.Users.FindAsync(receiverId);

                            if (receiver != null && !string.IsNullOrEmpty(receiver.Email))
                            {
                                await emailService.SendNewMessageNotificationAsync(
                                    receiver.Email,
                                    receiver.Name,
                                    sender?.Name ?? "Someone",
                                    content.Length > 100 ? content.Substring(0, 100) + "..." : content
                                );
                                Console.WriteLine("✅ Email enviado com sucesso!");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erro ao enviar email: {ex.Message}");
                    }
                });
                
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