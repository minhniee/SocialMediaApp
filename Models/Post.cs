using SocialMediaApp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class Post
    {
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }

        public Post()
        {
            Comments = new HashSet<Comment>();
            Likes = new HashSet<Like>();
        }
    }
}

