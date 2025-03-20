using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

