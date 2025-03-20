using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
    public enum ProfileVisibility
    {
        Everyone,
        FriendsOnly,
        OnlyMe
    }

    public class PrivacySettingsViewModel
    {
        [Display(Name = "Who can see my profile")]
        public ProfileVisibility ProfileVisibility { get; set; }
        
        [Display(Name = "Allow friend requests")]
        public bool AllowFriendRequests { get; set; }
        
        [Display(Name = "Show when I'm online")]
        public bool ShowOnlineStatus { get; set; }
        
        [Display(Name = "Allow others to tag me in posts")]
        public bool AllowTagging { get; set; }
    }
}

