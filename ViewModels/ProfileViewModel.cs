using System.Collections.Generic;

namespace SocialMediaApp.ViewModels
{
    public enum FriendStatus
    {
        NotFriends,
        RequestSent,
        RequestReceived,
        Friends
    }

    public class ProfileViewModel
    {
        public UserViewModel User { get; set; }
        public List<PostViewModel> Posts { get; set; } = new List<PostViewModel>();
        public FriendStatus FriendStatus { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}

