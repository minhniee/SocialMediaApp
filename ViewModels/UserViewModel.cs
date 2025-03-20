using System;
using System.Collections.Generic;

namespace SocialMediaApp.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Bio { get; set; }
        public DateTime JoinDate { get; set; }
        public int PostCount { get; set; }
        public int FriendCount { get; set; }
        public List<UserViewModel> Friends { get; set; } = new List<UserViewModel>();
    }
}

