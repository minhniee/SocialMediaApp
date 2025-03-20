using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
	public class EditProfileViewModel
	{
		[Required]
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Display(Name = "Bio")]
		[StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
		public string Bio { get; set; }

		[Display(Name = "Date of Birth")]
		[DataType(DataType.Date)]
		public DateTime DateOfBirth { get; set; }

		[Display(Name = "Profile Picture")]
		public IFormFile? ProfilePicture { get; set; }

		public string? CurrentProfilePictureUrl { get; set; }



		[DataType(DataType.Password)]
		[Display(Name = "Current Password")]
		public string? CurrentPassword { get; set; }

		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New Password")]
		public string? NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm New Password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string? ConfirmNewPassword { get; set; }
	}
}

