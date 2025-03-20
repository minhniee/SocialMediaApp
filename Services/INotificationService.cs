using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Services
{
    public interface INotificationService
    {
        Task CreateLikeNotificationAsync(int postId, string likerId);
        Task CreateCommentNotificationAsync(Comment comment);
        Task CreateFriendRequestNotificationAsync(string senderId, string receiverId);
        Task CreateFriendAcceptedNotificationAsync(string accepterId, string requesterId);
        Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId, int count = 20);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task MarkAllNotificationsAsReadAsync(string userId);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task<NotificationViewModel> GetNotificationByIdAsync(int notificationId);
    }
}

