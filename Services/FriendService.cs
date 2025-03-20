using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Services
{
    public class FriendService : IFriendService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public FriendService(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<UserViewModel> GetUserProfileAsync(string userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            // Get post count
            var postCount = await _context.Posts.CountAsync(p => p.UserId == userId);

            // Get friend count
            var friendCount = await _context.Friendships.CountAsync(f => f.UserId == userId);

            // Get friends for display (limited to 9 for UI)
            var friends = await _context.Friendships
                .Where(f => f.UserId == userId)
                .Include(f => f.Friend)
                .OrderBy(f => f.CreatedAt)
                .Take(9)
                .Select(f => new UserViewModel
                {
                    Id = f.Friend.Id,
                    UserName = f.Friend.UserName,
                    FirstName = f.Friend.FirstName,
                    LastName = f.Friend.LastName,
                    FullName = $"{f.Friend.FirstName} {f.Friend.LastName}",
                    ProfilePictureUrl = f.Friend.ProfilePictureUrl
                })
                .ToListAsync();

            return new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                JoinDate = user.JoinDate,
                PostCount = postCount,
                FriendCount = friendCount,
                Friends = friends
            };
        }

        public async Task<FriendStatus> GetFriendStatusAsync(string currentUserId, string otherUserId)
        {
            // If it's the same user
            if (currentUserId == otherUserId)
            {
                return FriendStatus.NotFriends;
            }

            // Check if they are already friends
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == currentUserId && f.FriendId == otherUserId);

            if (friendship != null)
            {
                return FriendStatus.Friends;
            }

            // Check for pending friend requests
            var sentRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == currentUserId && fr.ReceiverId == otherUserId && fr.Status == FriendRequestStatus.Pending);

            if (sentRequest != null)
            {
                return FriendStatus.RequestSent;
            }

            var receivedRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == otherUserId && fr.ReceiverId == currentUserId && fr.Status == FriendRequestStatus.Pending);

            if (receivedRequest != null)
            {
                return FriendStatus.RequestReceived;
            }

            return FriendStatus.NotFriends;
        }

        public async Task SendFriendRequestAsync(string senderId, string receiverId)
        {
            // Check if a request already exists
            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr =>
                    fr.SenderId == senderId && fr.ReceiverId == receiverId ||
                    fr.SenderId == receiverId && fr.ReceiverId == senderId);

            if (existingRequest != null)
            {
                // If the request was previously rejected, update it to pending
                if (existingRequest.Status == FriendRequestStatus.Rejected &&
                    existingRequest.SenderId == senderId)
                {
                    existingRequest.Status = FriendRequestStatus.Pending;
                    existingRequest.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Create notification for the receiver
                    await _notificationService.CreateFriendRequestNotificationAsync(senderId, receiverId);
                }
                return;
            }

            // Check if they are already friends
            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    f.UserId == senderId && f.FriendId == receiverId ||
                    f.UserId == receiverId && f.FriendId == senderId);

            if (existingFriendship != null)
            {
                return;
            }

            // Create new friend request
            var friendRequest = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();

            // Create notification for the receiver
            await _notificationService.CreateFriendRequestNotificationAsync(senderId, receiverId);
        }

        public async Task AcceptFriendRequestAsync(string senderId, string receiverId)
        {
            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.Status == FriendRequestStatus.Pending);

            if (friendRequest == null)
            {
                return;
            }

            // Update friend request status
            friendRequest.Status = FriendRequestStatus.Accepted;
            friendRequest.UpdatedAt = DateTime.UtcNow;

            // Create bidirectional friendship
            var friendship1 = new Friendship
            {
                UserId = senderId,
                FriendId = receiverId,
                CreatedAt = DateTime.UtcNow
            };

            var friendship2 = new Friendship
            {
                UserId = receiverId,
                FriendId = senderId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship1);
            _context.Friendships.Add(friendship2);

            await _context.SaveChangesAsync();

            // Create notification for the sender
            await _notificationService.CreateFriendAcceptedNotificationAsync(receiverId, senderId);
        }

        public async Task RejectFriendRequestAsync(string senderId, string receiverId)
        {
            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.Status == FriendRequestStatus.Pending);

            if (friendRequest == null)
            {
                return;
            }

            // Update friend request status
            friendRequest.Status = FriendRequestStatus.Rejected;
            friendRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveFriendAsync(string userId, string friendId)
        {
            // Find both friendship records (bidirectional)
            var friendship1 = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == friendId);

            var friendship2 = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == userId);

            if (friendship1 != null)
            {
                _context.Friendships.Remove(friendship1);
            }

            if (friendship2 != null)
            {
                _context.Friendships.Remove(friendship2);
            }

            // Also find and update any friend requests
            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr =>
                    fr.SenderId == userId && fr.ReceiverId == friendId ||
                    fr.SenderId == friendId && fr.ReceiverId == userId);

            if (friendRequest != null)
            {
                // Mark as rejected
                friendRequest.Status = FriendRequestStatus.Rejected;
                friendRequest.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<UserViewModel>> GetFriendSuggestionsAsync(string userId, int count = 5)
        {
            // Get current user's friends
            var friendIds = await _context.Friendships
                .Where(f => f.UserId == userId)
                .Select(f => f.FriendId)
                .ToListAsync();

            // Add the current user to exclude them from suggestions
            friendIds.Add(userId);

            // Get pending friend requests
            var pendingRequestUserIds = await _context.FriendRequests
                .Where(fr => (fr.SenderId == userId || fr.ReceiverId == userId) && fr.Status == FriendRequestStatus.Pending)
                .Select(fr => fr.SenderId == userId ? fr.ReceiverId : fr.SenderId)
                .ToListAsync();

            // Add these to the exclusion list
            friendIds.AddRange(pendingRequestUserIds);

            // Find friends of friends who aren't already friends with the user
            var friendsOfFriends = await _context.Friendships
                .Where(f => friendIds.Contains(f.UserId) && !friendIds.Contains(f.FriendId))
                .Select(f => f.FriendId)
                .Distinct()
                .Take(count)
                .ToListAsync();

            // If we don't have enough suggestions, add some random users
            if (friendsOfFriends.Count < count)
            {
                var randomUsers = await _context.Users
                    .Where(u => !friendIds.Contains(u.Id) && !friendsOfFriends.Contains(u.Id))
                    .OrderBy(u => Guid.NewGuid()) // Random ordering
                    .Take(count - friendsOfFriends.Count)
                    .Select(u => u.Id)
                    .ToListAsync();

                friendsOfFriends.AddRange(randomUsers);
            }

            // Get user details for suggestions
            var suggestions = await _context.Users
                .Where(u => friendsOfFriends.Contains(u.Id))
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = $"{u.FirstName} {u.LastName}",
                    ProfilePictureUrl = u.ProfilePictureUrl
                })
                .ToListAsync();

            return suggestions;
        }

        public async Task<List<UserViewModel>> GetUserFriendsAsync(string userId)
        {
            var friends = await _context.Friendships
                .Where(f => f.UserId == userId)
                .Include(f => f.Friend)
                .OrderBy(f => f.Friend.FirstName)
                .ThenBy(f => f.Friend.LastName)
                .Select(f => new UserViewModel
                {
                    Id = f.Friend.Id,
                    UserName = f.Friend.UserName,
                    FirstName = f.Friend.FirstName,
                    LastName = f.Friend.LastName,
                    FullName = $"{f.Friend.FirstName} {f.Friend.LastName}",
                    ProfilePictureUrl = f.Friend.ProfilePictureUrl
                })
                .ToListAsync();

            return friends;
        }

        public async Task<List<FriendRequest>> GetPendingFriendRequestsAsync(string userId)
        {
            var pendingRequests = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                .Include(fr => fr.Sender)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            return pendingRequests;
        }
    }
}

