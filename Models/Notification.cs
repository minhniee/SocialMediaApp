using SocialMediaApp.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public enum NotificationType
    {
        Like,
        Comment,
        FriendRequest,
        FriendAccepted
    }

    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        public string SourceUserId { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        [Required]
        public NotificationType Type { get; set; }
        
        public int? ReferenceId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationUser SourceUser { get; set; }
    }
}

