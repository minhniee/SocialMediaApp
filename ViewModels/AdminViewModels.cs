using SocialMediaApp.Services;

namespace SocialMediaApp.ViewModels
{
    public class AdminDashboardViewModel
    {
        public UserStatisticsViewModel UserStatistics { get; set; }
        public ContentStatisticsViewModel ContentStatistics { get; set; }
        public List<TrafficDataPoint> TrafficData { get; set; }
        public TimeRange TimeRange { get; set; }
    }

    public class UserStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int NewUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
    }

    public class ContentStatisticsViewModel
    {
        public int TotalPosts { get; set; }
        public int NewPosts { get; set; }
        public int TotalComments { get; set; }
        public int NewComments { get; set; }
        public int TotalLikes { get; set; }
        public int NewLikes { get; set; }
    }

    public class TrafficDataPoint
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    public class UserManagementViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public List<PostViewModel> RecentPosts { get; set; } = new List<PostViewModel>();
    }

    public class PostModerationViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserViewModel User { get; set; }
        public int ReportCount { get; set; }
        public string Status { get; set; }
    }

    public class ChangeUserPasswordViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> CurrentRoles { get; set; } = new List<string>();
        public List<string> AvailableRoles { get; set; } = new List<string>();
        public string SelectedRole { get; set; }
    }
}

