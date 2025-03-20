namespace SocialMediaApp.ViewModels
{
    public class HomeViewModel
    {
        public List<PostViewModel> Posts { get; set; } = new List<PostViewModel>();
        public PostViewModel NewPost { get; set; }

        public List<UserViewModel> FriendModel { get; set; }
        //public List<IdentityUser> User { get; set; } = new List<IdentityUser>();
        // public

    }
}

