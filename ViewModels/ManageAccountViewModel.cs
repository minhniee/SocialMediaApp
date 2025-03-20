using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
    public class ManageAccountViewModel
    {
        public string Email { get; set; }
        
        public bool IsEmailConfirmed { get; set; }
        
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
        
        public bool TwoFactorEnabled { get; set; }
        
        public bool HasAuthenticator { get; set; }
    }
}

