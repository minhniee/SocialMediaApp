using SocialMediaApp.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class Like
    {
        public int Id { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public int PostId { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        // Navigation properties
        public virtual Post Post { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}

