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

        // NOVO: Recebe o animalId
        public async Task SendChatMessage(int receiverId, int? animalId, string content)
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
                    AnimalListingId = animalId, // NOVO: Guarda o contexto do animal!
                    Content = content.Trim(),
                    Timestamp = DateTime.Now,
                    IsRead = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

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
                           }
                       }
                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine($"❌ Erro ao enviar email: {ex.Message}");
                   }
               });

                // 3. Identificar o Grupo (Agora separado pelo Animal)
                string groupName = GetGroupName(senderId, receiverId, animalId);

                // 4. Notificar TODOS no grupo
                await Clients.Group(groupName).SendAsync("ReceiveMessage",
                    senderId,
                    message.Content,
                    message.Timestamp.ToString("HH:mm"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR Error]: {ex.Message}");
                throw;
            }
        }

        // NOVO: O JoinChat também tem de saber de que animal estamos a falar
        public async Task JoinChat(int userId1, int userId2, int? animalId)
        {
            string groupName = GetGroupName(userId1, userId2, animalId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"[SignalR]: User {Context.ConnectionId} joined {groupName}");
        }

        // NOVO: A lógica que cria o nome da sala de forma única
        private string GetGroupName(int id1, int id2, int? animalId)
        {
            var list = new List<int> { id1, id2 };
            list.Sort();

            // Se houver animal, a sala é "chat_3_5_pet_12". Se não houver, é "chat_3_5".
            return animalId.HasValue
                ? $"chat_{list[0]}_{list[1]}_pet_{animalId.Value}"
                : $"chat_{list[0]}_{list[1]}";
        }
    }
}