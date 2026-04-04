using Microsoft.EntityFrameworkCore;
using PetLink.Data;
using PetLink.Models;

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

    }
}
