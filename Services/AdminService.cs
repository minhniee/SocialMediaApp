using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<AdminDashboardViewModel> GetDashboardDataAsync(TimeRange timeRange)
        {
            var startDate = GetStartDateFromTimeRange(timeRange);

            var userStats = await GetUserStatisticsAsync(timeRange);
            var contentStats = await GetContentStatisticsAsync(timeRange);

            var trafficData = await GetTrafficDataAsync(startDate, timeRange);

            return new AdminDashboardViewModel
            {
                UserStatistics = userStats,
                ContentStatistics = contentStats,
                TrafficData = trafficData,
                TimeRange = timeRange
            };
        }

        public async Task<List<UserManagementViewModel>> GetUsersAsync(string searchTerm = null, int page = 1, int pageSize = 10)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    u.UserName.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    u.FirstName.Contains(searchTerm) ||
                    u.LastName.Contains(searchTerm));
            }

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userViewModels = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var lockoutEnd = user.LockoutEnd;
                var isLockedOut = lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow;

                userViewModels.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = $"{user.FirstName} {user.LastName}",
                    JoinDate = user.JoinDate,
                    IsLockedOut = isLockedOut,
                    LockoutEnd = user.LockoutEnd,
                    Roles = roles.ToList(),
                    PostCount = await _context.Posts.CountAsync(p => p.UserId == user.Id),
                    CommentCount = await _context.Comments.CountAsync(c => c.UserId == user.Id)
                });
            }

            return userViewModels;
        }

        public async Task<UserManagementViewModel> GetUserDetailsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var lockoutEnd = user.LockoutEnd;
            var isLockedOut = lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow;

            return new UserManagementViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                JoinDate = user.JoinDate,
                IsLockedOut = isLockedOut,
                LockoutEnd = user.LockoutEnd,
                Roles = roles.ToList(),
                PostCount = await _context.Posts.CountAsync(p => p.UserId == user.Id),
                CommentCount = await _context.Comments.CountAsync(c => c.UserId == user.Id),
                RecentPosts = await _context.Posts
                    .Where(p => p.UserId == user.Id)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => new PostViewModel
                    {
                        Id = p.Id,
                        Content = p.Content,
                        CreatedAt = p.CreatedAt,
                        LikeCount = p.Likes.Count,
                        // CommentCount = p.Comments.Count
                    })
                    .ToListAsync()
            };
        }

        public async Task<bool> LockUserAccountAsync(string userId, DateTimeOffset? endDate = null)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            // If no end date is provided, lock for 100 years (effectively permanent)
            if (!endDate.HasValue)
            {
                endDate = DateTimeOffset.UtcNow.AddYears(100);
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, endDate);

            return result.Succeeded;
        }

        public async Task<bool> UnlockUserAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, null);

            return result.Succeeded;
        }

        public async Task<bool> ChangeUserPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            // Remove existing password if any
            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                await _userManager.RemovePasswordAsync(user);
            }

            // Add new password
            var result = await _userManager.AddPasswordAsync(user, newPassword);

            return result.Succeeded;
        }

        public async Task<List<PostModerationViewModel>> GetFlaggedPostsAsync(int page = 1, int pageSize = 10)
        {
            // In a real application, you would have a flagging system
            // For this example, we'll just get the most recent posts
            var posts = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostModerationViewModel
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    User = new UserViewModel
                    {
                        Id = p.User.Id,
                        UserName = p.User.UserName,
                        FullName = $"{p.User.FirstName} {p.User.LastName}",
                        ProfilePictureUrl = p.User.ProfilePictureUrl
                    },
                    ReportCount = 0, // In a real app, this would be the count of reports
                    Status = "Active" // In a real app, this would be the moderation status
                })
                .ToListAsync();

            return posts;
        }

        public async Task<bool> ModeratePostAsync(int postId, ModerationAction action, string reason)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return false;
            }

            switch (action)
            {
                case ModerationAction.Approve:
                    // Mark as approved in a real app
                    break;
                case ModerationAction.Hide:
                    // Hide the post in a real app
                    break;
                case ModerationAction.Delete:
                    _context.Posts.Remove(post);
                    break;
                case ModerationAction.Flag:
                    // Flag for further review in a real app
                    break;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserStatisticsViewModel> GetUserStatisticsAsync(TimeRange timeRange)
        {
            var startDate = GetStartDateFromTimeRange(timeRange);

            var totalUsers = await _userManager.Users.CountAsync();
            var newUsers = await _userManager.Users.CountAsync(u => u.JoinDate >= startDate);
            var activeUsers = await _context.Posts
                .Where(p => p.CreatedAt >= startDate)
                .Select(p => p.UserId)
                .Distinct()
                .CountAsync();

            return new UserStatisticsViewModel
            {
                TotalUsers = totalUsers,
                NewUsers = newUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = totalUsers - activeUsers
            };
        }

        public async Task<ContentStatisticsViewModel> GetContentStatisticsAsync(TimeRange timeRange)
        {
            var startDate = GetStartDateFromTimeRange(timeRange);

            var totalPosts = await _context.Posts.CountAsync();
            var newPosts = await _context.Posts.CountAsync(p => p.CreatedAt >= startDate);
            var totalComments = await _context.Comments.CountAsync();
            var newComments = await _context.Comments.CountAsync(c => c.CreatedAt >= startDate);
            var totalLikes = await _context.Likes.CountAsync();
            var newLikes = await _context.Likes.CountAsync(l => l.CreatedAt >= startDate);

            return new ContentStatisticsViewModel
            {
                TotalPosts = totalPosts,
                NewPosts = newPosts,
                TotalComments = totalComments,
                NewComments = newComments,
                TotalLikes = totalLikes,
                NewLikes = newLikes
            };
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            // Create role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Add user to role
            var result = await _userManager.AddToRoleAsync(user, roleName);

            return result.Succeeded;
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || !await _roleManager.RoleExistsAsync(roleName))
            {
                return false;
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            return result.Succeeded;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new List<string>();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return roles.ToList();
        }

        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        }

        private async Task<List<TrafficDataPoint>> GetTrafficDataAsync(DateTime startDate, TimeRange timeRange)
        {
            var result = new List<TrafficDataPoint>();

            // Get login data (in a real app, you would track logins)
            // For this example, we'll use post creation as a proxy for activity
            var posts = await _context.Posts
                .Where(p => p.CreatedAt >= startDate)
                .ToListAsync();

            switch (timeRange)
            {
                case TimeRange.Day:
                    // Group by hour
                    result = posts
                        .GroupBy(p => p.CreatedAt.Hour)
                        .Select(g => new TrafficDataPoint
                        {
                            Label = $"{g.Key}:00",
                            Value = g.Count()
                        })
                        .OrderBy(p => p.Label)
                        .ToList();
                    break;

                case TimeRange.Week:
                    // Group by day of week
                    result = posts
                        .GroupBy(p => p.CreatedAt.DayOfWeek)
                        .Select(g => new TrafficDataPoint
                        {
                            Label = g.Key.ToString(),
                            Value = g.Count()
                        })
                        .OrderBy(p => p.Label)
                        .ToList();
                    break;

                case TimeRange.Month:
                    // Group by day of month
                    result = posts
                        .GroupBy(p => p.CreatedAt.Day)
                        .Select(g => new TrafficDataPoint
                        {
                            Label = g.Key.ToString(),
                            Value = g.Count()
                        })
                        .OrderBy(p => p.Label)
                        .ToList();
                    break;

                case TimeRange.Year:
                    // Group by month
                    result = posts
                        .GroupBy(p => p.CreatedAt.Month)
                        .Select(g => new TrafficDataPoint
                        {
                            Label = new DateTime(2000, g.Key, 1).ToString("MMM"),
                            Value = g.Count()
                        })
                        .OrderBy(p => p.Label)
                        .ToList();
                    break;
            }

            return result;
        }

        private DateTime GetStartDateFromTimeRange(TimeRange timeRange)
        {
            var now = DateTime.UtcNow;

            return timeRange switch
            {
                TimeRange.Day => now.AddDays(-1),
                TimeRange.Week => now.AddDays(-7),
                TimeRange.Month => now.AddMonths(-1),
                TimeRange.Year => now.AddYears(-1),
                _ => now.AddDays(-30)
            };
        }
    }
}

