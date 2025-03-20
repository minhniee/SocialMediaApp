using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Services
{
    public interface IFriendService
    {
        Task<UserViewModel> GetUserProfileAsync(string userId);
        Task<FriendStatus> GetFriendStatusAsync(string currentUserId, string otherUserId);
        Task SendFriendRequestAsync(string senderId, string receiverId);
        Task AcceptFriendRequestAsync(string senderId, string receiverId);
        Task RejectFriendRequestAsync(string senderId, string receiverId);
        Task RemoveFriendAsync(string userId, string friendId);
        Task<List<UserViewModel>> GetFriendSuggestionsAsync(string userId, int count = 5);
        Task<List<UserViewModel>> GetUserFriendsAsync(string userId);
        Task<List<FriendRequest>> GetPendingFriendRequestsAsync(string userId);
    }
}

