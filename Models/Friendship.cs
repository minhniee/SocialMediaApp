using SocialMediaApp.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class Friendship
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string FriendId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationUser Friend { get; set; }
    }
}

