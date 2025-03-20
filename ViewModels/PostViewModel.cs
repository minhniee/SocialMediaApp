using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.ViewModels
{
	public class PostViewModel
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Please enter some content for your post.")]
		public string Content { get; set; }

		public string ImageUrl { get; set; }

		public DateTime CreatedAt { get; set; }

		public UserViewModel? User { get; set; }

		public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();

		public int LikeCount { get; set; } = 0;

		public bool IsLikedByCurrentUser { get; set; }

		// For creating new posts
		[NotMapped]
		public IFormFile? Image { get; set; }
	}
}

