using PetLink.Models;

namespace PetLink.Hubs
{
    public interface INotificationService
    {

        Task CreateListingStatusNotificationAsync(int userId, string petName, int listingId, string oldStatus, string newStatus);
        Task<List<ListingsNotification>> GetUserUnreadNotificationsAsync(int userId);
        Task<List<ListingsNotification>> GetUserRecentNotificationsAsync(int userId, int count = 10);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);

    }
}
