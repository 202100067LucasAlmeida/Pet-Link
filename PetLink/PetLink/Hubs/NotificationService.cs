using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;
using PetLink.Models.Enums;

namespace PetLink.Hubs
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        //criar aa notificação em si
        public async Task CreateListingStatusNotificationAsync(int userId, string petName, int listingId, string oldStatus, string newStatus)
        {
            var notification = new ListingsNotification
            {
                UserId = userId,
                AnimalListingId = listingId,
                Title = $"UPDATE on {petName}",
                Message = $"{petName}'s listing status has changed from {oldStatus} to {newStatus}.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.ListingsNotifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        //vamos buscar as notificações que ainda NÃO foram lidas
        public async Task<List<ListingsNotification>> GetUserUnreadNotificationsAsync(int userId)
        {
            return await _context.ListingsNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .Include(n => n.AnimalListing)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        //"reunimos" as notificações todas
        public async Task<List<ListingsNotification>> GetUserRecentNotificationsAsync(int userId, int count = 10)
        {
            return await _context.ListingsNotifications
                .Where(n => n.UserId == userId)
                .Include(n => n.AnimalListing)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        //marcar uma como lida
        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.ListingsNotifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        //marcar TODAS como lidas (pq somos preguiçosos pra ler tudo)
        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.ListingsNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.ListingsNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }


        //ADMIN NOTIFICATIONS

        public async Task CreateNewListingNotificationForAdminsAsync(int listingId, string petName, int tutorId)
        {
            // Get all admin users
            var admins = await _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .ToListAsync();

            foreach (var admin in admins)
            {
                var notification = new ListingsNotification
                {
                    UserId = admin.Id,
                    AnimalListingId = listingId,
                    Title = $"New Listing Awaiting Review: {petName}",
                    Message = $"A new pet listing '{petName}' has been created and requires your review. Please approve or reject it.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ListingsNotifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }

       
        public async Task CreateNewUserNotificationForAdminsAsync(int userId, string userName, string userEmail, UserRole userRole)
        {
            
            var admins = await _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .ToListAsync();

            //vou deixar aqui como exemplo, caso queiramos usar no futuro
            //bool needsVerification = (userRole == UserRole.Shelter);


            foreach (var admin in admins)
            {
                var notification = new ListingsNotification
                {
                    UserId = admin.Id,
                    AnimalListingId = null,
                    Title = $"New User Registration: {userName}",
                    Message = $"A new unverified {userRole} account has been created. User: {userName} ({userEmail})",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ListingsNotifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }

        public async Task CreateNewEventNotificationForAdminsAsync(int eventId, string eventName, int organizerId)
        {
            // Buscar todos os admins
            var admins = await _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .ToListAsync();

            var organizer = await _context.Users.FindAsync(organizerId);

            foreach (var admin in admins)
            {
                var notification = new ListingsNotification
                {
                    UserId = admin.Id,
                    Title = "New Event Pending Approval",
                    Message = $"A new event '{eventName}' has been created by {organizer?.Name} and needs your review.",
                    AnimalListingId = null,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.ListingsNotifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }

        public async Task CreateEventApprovalNotificationAsync(int organizerId, string eventName, int eventId, bool isApproved, string rejectionReason = null)
        {
            string title;
            string message;
    
        if (isApproved)
        {
            title = "Event Approved! 🎉";
            message = $"Your event '{eventName}' has been approved and is now visible to the public.";
        }
        {
            title = "Event Rejected ❌";  // Mudar de "Event Update" para "Event Rejected ❌"
            if (!string.IsNullOrEmpty(rejectionReason))
            {
                message = $"Your event '{eventName}' has been rejected. Reason: {rejectionReason}";
            }
            else
            {
                message = $"Your event '{eventName}' has been rejected. Please check the requirements and try again.";
            }
        }

        var notification = new ListingsNotification
        {
            UserId = organizerId,
            Title = title,
            Message = message,
            AnimalListingId = eventId,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _context.ListingsNotifications.Add(notification);
        await _context.SaveChangesAsync();
    }

        public async Task CreateNewEventInterestNotificationAsync(int organizerId, string eventName, int eventId, int userId, string userName)
    {
        // Notificar o organizador
        var notification = new ListingsNotification
        {
            UserId = organizerId,
            Title = "New Event Enrollment! 🎯",
            Message = $"User {userName} has registered for your event '{eventName}'.",
            AnimalListingId = null,
            IsRead = false,
            CreatedAt = DateTime.Now
        };
        _context.ListingsNotifications.Add(notification);

        // Notificar o utilizador que se registou 
        var userNotification = new ListingsNotification
        {
            UserId = userId,
            Title = "Event Registration Confirmed ✅",
            Message = $"You are now registered for '{eventName}'. We'll keep you updated!",
            AnimalListingId = null,
            IsRead = false,
            CreatedAt = DateTime.Now
        };
        _context.ListingsNotifications.Add(userNotification);

        await _context.SaveChangesAsync();
    }

    // Método para notificações de proximidade da data do evento
    public async Task SendEventReminderNotificationsAsync()
    {
        var upcomingEvents = await _context.Events
            .Where(e => e.Status == EventStatus.Approved && e.StartDate > DateTime.Now)
            .ToListAsync();

        foreach (var eventItem in upcomingEvents)
        {
            // Verificar se está a 1 dia ou 1 hora do evento
            var timeUntilEvent = eventItem.StartDate - DateTime.Now;
            
            if (timeUntilEvent.TotalDays <= 1 && timeUntilEvent.TotalDays > 0)
            {
                // Enviar notificação para todos os interessados
                var interestedUsers = await _context.EventInterests
                    .Where(ei => ei.EventId == eventItem.Id)
                    .Include(ei => ei.User)
                    .ToListAsync();

                foreach (var interest in interestedUsers)
                {
                    var notification = new ListingsNotification
                    {
                        UserId = interest.UserId,
                        Title = $"Event Reminder: {eventItem.Name} is tomorrow! ⏰",
                        Message = $"Don't forget! '{eventItem.Name}' is happening tomorrow at {eventItem.StartDate.ToString("HH:mm")} at {eventItem.Location}.",
                        AnimalListingId = null,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.ListingsNotifications.Add(notification);
                }
            }
            else if (timeUntilEvent.TotalHours <= 1 && timeUntilEvent.TotalHours > 0)
            {
                // Enviar notificação para todos os interessados (1 hora antes)
                var interestedUsers = await _context.EventInterests
                    .Where(ei => ei.EventId == eventItem.Id)
                    .Include(ei => ei.User)
                    .ToListAsync();

                foreach (var interest in interestedUsers)
                {
                    var notification = new ListingsNotification
                    {
                        UserId = interest.UserId,
                        Title = $"Event Starting Soon: {eventItem.Name}! ⏰",
                        Message = $"'{eventItem.Name}' is starting in less than an hour at {eventItem.Location}! See you there! 🎉",
                        AnimalListingId = null,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.ListingsNotifications.Add(notification);
                }
            }

            
        }

        await _context.SaveChangesAsync();
    }

    
    }
}
