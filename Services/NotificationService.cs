using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Hubs;
using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            ApplicationDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateLikeNotificationAsync(int postId, string likerId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null || post.UserId == likerId)
            {
                return; // Don't notify if post doesn't exist or user liked their own post
            }

            var liker = await _context.Users.FindAsync(likerId);
            if (liker == null)
            {
                return;
            }

            var notification = new Notification
            {
                UserId = post.UserId,
                SourceUserId = likerId,
                Message = $"{liker.FirstName} {liker.LastName} liked your post",
                Type = NotificationType.Like,
                ReferenceId = postId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification
            await SendRealTimeNotificationAsync(notification);
        }

        public async Task CreateCommentNotificationAsync(Comment comment)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == comment.PostId);

            if (post == null || post.UserId == comment.UserId)
            {
                return; // Don't notify if post doesn't exist or user commented on their own post
            }

            var commenter = await _context.Users.FindAsync(comment.UserId);
            if (commenter == null)
            {
                return;
            }

            var notification = new Notification
            {
                UserId = post.UserId,
                SourceUserId = comment.UserId,
                Message = $"{commenter.FirstName} {commenter.LastName} commented on your post",
                Type = NotificationType.Comment,
                ReferenceId = comment.PostId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification
            await SendRealTimeNotificationAsync(notification);
        }

        public async Task CreateFriendRequestNotificationAsync(string senderId, string receiverId)
        {
            var sender = await _context.Users.FindAsync(senderId);
            if (sender == null)
            {
                return;
            }

            var notification = new Notification
            {
                UserId = receiverId,
                SourceUserId = senderId,
                Message = $"{sender.FirstName} {sender.LastName} sent you a friend request",
                Type = NotificationType.FriendRequest,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification
            await SendRealTimeNotificationAsync(notification);
        }

        public async Task CreateFriendAcceptedNotificationAsync(string accepterId, string requesterId)
        {
            var accepter = await _context.Users.FindAsync(accepterId);
            if (accepter == null)
            {
                return;
            }

            var notification = new Notification
            {
                UserId = requesterId,
                SourceUserId = accepterId,
                Message = $"{accepter.FirstName} {accepter.LastName} accepted your friend request",
                Type = NotificationType.FriendAccepted,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification
            await SendRealTimeNotificationAsync(notification);
        }

        public async Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId, int count = 20)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.SourceUser)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    Type = n.Type,
                    ReferenceId = n.ReferenceId,
                    SourceUser = n.SourceUser != null ? new UserViewModel
                    {
                        Id = n.SourceUser.Id,
                        UserName = n.SourceUser.UserName,
                        FirstName = n.SourceUser.FirstName,
                        LastName = n.SourceUser.LastName,
                        FullName = $"{n.SourceUser.FirstName} {n.SourceUser.LastName}",
                        ProfilePictureUrl = n.SourceUser.ProfilePictureUrl
                    } : null
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<NotificationViewModel> GetNotificationByIdAsync(int notificationId)
        {
            var notification = await _context.Notifications
                .Include(n => n.SourceUser)
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null)
            {
                return null;
            }

            return new NotificationViewModel
            {
                Id = notification.Id,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead,
                Type = notification.Type,
                ReferenceId = notification.ReferenceId,
                SourceUser = notification.SourceUser != null ? new UserViewModel
                {
                    Id = notification.SourceUser.Id,
                    UserName = notification.SourceUser.UserName,
                    FirstName = notification.SourceUser.FirstName,
                    LastName = notification.SourceUser.LastName,
                    FullName = $"{notification.SourceUser.FirstName} {notification.SourceUser.LastName}",
                    ProfilePictureUrl = notification.SourceUser.ProfilePictureUrl
                } : null
            };
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();

                // Update the unread count for the user
                var unreadCount = await GetUnreadNotificationCountAsync(notification.UserId);
                await _hubContext.Clients.Group(notification.UserId).SendAsync("UpdateUnreadCount", unreadCount);
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            // Update the unread count for the user (should be 0 now)
            await _hubContext.Clients.Group(userId).SendAsync("UpdateUnreadCount", 0);
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        private async Task SendRealTimeNotificationAsync(Notification notification)
        {
            // Get the notification view model to send to the client
            var notificationViewModel = await GetNotificationByIdAsync(notification.Id);

            // Get the unread count for the user
            var unreadCount = await GetUnreadNotificationCountAsync(notification.UserId);

            // Send the notification to the specific user's group
            await _hubContext.Clients.Group(notification.UserId).SendAsync("ReceiveNotification", notificationViewModel);

            // Update the unread count
            await _hubContext.Clients.Group(notification.UserId).SendAsync("UpdateUnreadCount", unreadCount);
        }
    }
}

