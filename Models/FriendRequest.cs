using SocialMediaApp.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public enum FriendRequestStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public class FriendRequest
    {
        public int Id { get; set; }
        
        [Required]
        public string SenderId { get; set; }
        
        [Required]
        public string ReceiverId { get; set; }
        
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser Sender { get; set; }
        public virtual ApplicationUser Receiver { get; set; }
    }
}

