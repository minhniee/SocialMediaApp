using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
    public class DeleteAccountViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string Password { get; set; }
        
        [Required]
        [Display(Name = "I understand this action cannot be undone")]
        public bool ConfirmDelete { get; set; }
    }
}

