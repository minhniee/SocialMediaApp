using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
    public class ChangeEmailViewModel
    {
        [EmailAddress]
        [Display(Name = "Current Email")]
        public string Email { get; set; }
        
        [Required]
        [EmailAddress]
        [Display(Name = "New Email")]
        public string NewEmail { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}

