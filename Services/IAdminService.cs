using SocialMediaApp.Data;
using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardDataAsync(TimeRange timeRange);
        Task<List<UserManagementViewModel>> GetUsersAsync(string searchTerm = null, int page = 1, int pageSize = 10);
        Task<UserManagementViewModel> GetUserDetailsAsync(string userId);
        Task<bool> LockUserAccountAsync(string userId, DateTimeOffset? endDate = null);
        Task<bool> UnlockUserAccountAsync(string userId);
        Task<bool> ChangeUserPasswordAsync(string userId, string newPassword);
        Task<List<PostModerationViewModel>> GetFlaggedPostsAsync(int page = 1, int pageSize = 10);
        Task<bool> ModeratePostAsync(int postId, ModerationAction action, string reason);
        Task<UserStatisticsViewModel> GetUserStatisticsAsync(TimeRange timeRange);
        Task<ContentStatisticsViewModel> GetContentStatisticsAsync(TimeRange timeRange);
        Task<bool> AssignRoleToUserAsync(string userId, string roleName);
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleName);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<List<string>> GetAllRolesAsync();
    }

    public enum TimeRange
    {
        Day,
        Week,
        Month,
        Year
    }

    public enum ModerationAction
    {
        Approve,
        Hide,
        Delete,
        Flag
    }
}

