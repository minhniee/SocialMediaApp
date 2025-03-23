using Microsoft.AspNetCore.Identity;
using SocialMediaApp.Models;

namespace SocialMediaApp.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public string? Bio { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<FriendRequest> SentFriendRequests { get; set; } = new List<FriendRequest>();
        public virtual ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = new List<FriendRequest>();
        public virtual ICollection<Friendship> Friendships { get; set; } = new List<Friendship>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        //public ApplicationUser()
        //{
        //    Posts = new HashSet<Post>();
        //    Comments = new HashSet<Comment>();
        //    Likes = new HashSet<Like>();
        //    SentFriendRequests = new HashSet<FriendRequest>();
        //    ReceivedFriendRequests = new HashSet<FriendRequest>();
        //    Friendships = new HashSet<Friendship>();
        //    Notifications = new HashSet<Notification>();
        //}
    }
}

