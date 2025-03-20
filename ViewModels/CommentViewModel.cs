using System;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Comment content is required")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Content { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public int PostId { get; set; }
        
        public UserViewModel User { get; set; }
        
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;
                
                if (timeSpan.TotalMinutes < 1)
                    return "just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} min ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hr ago";
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} days ago";
                
                return CreatedAt.ToString("MMM d");
            }
        }
    }
}

