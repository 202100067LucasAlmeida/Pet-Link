using PetLink.Models;
using PetLink.Models.Enums;

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

        Task CreateNewListingNotificationForAdminsAsync(int listingId, string petName, int tutorId);

        Task CreateNewUserNotificationForAdminsAsync(int userId, string userName, string userEmail, UserRole userRole);

        Task CreateNewEventNotificationForAdminsAsync(int eventId, string eventName, int organizerId);
        Task CreateEventApprovalNotificationAsync(int organizerId, string eventName, int eventId, bool isApproved, string rejectionReason = null);

        Task CreateNewEventInterestNotificationAsync(int organizerId, string eventName, int eventId, int userId, string userName);

        Task SendEventReminderNotificationsAsync();
    }
    }

