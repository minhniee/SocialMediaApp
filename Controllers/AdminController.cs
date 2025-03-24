using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Data;
using SocialMediaApp.Services;
using SocialMediaApp.ViewModels;
using System.Threading.Tasks;

namespace SocialMediaApp.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            IAdminService adminService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _adminService = adminService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(TimeRange timeRange = TimeRange.Week)
        {
            var viewModel = await _adminService.GetDashboardDataAsync(timeRange);
            return View(viewModel);
        }

        public async Task<IActionResult> Users(string searchTerm = null, int page = 1)
        {
            const int pageSize = 10;
            var users = await _adminService.GetUsersAsync(searchTerm, page, pageSize);
            
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            
            return View(users);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _adminService.GetUserDetailsAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }
            
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id, int lockDuration = 0)
        {
            DateTimeOffset? lockoutEnd = null;
            
            if (lockDuration > 0)
            {
                lockoutEnd = DateTimeOffset.UtcNow.AddDays(lockDuration);
            }
            else if (lockDuration == -1)
            {
                // Permanent lock
                lockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }
            
            var result = await _adminService.LockUserAccountAsync(id, lockoutEnd);
            
            if (result)
            {
                TempData["StatusMessage"] = "User account has been locked.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to lock user account.";
            }
            
            return RedirectToAction(nameof(UserDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var result = await _adminService.UnlockUserAccountAsync(id);
            
            if (result)
            {
                TempData["StatusMessage"] = "User account has been unlocked.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to unlock user account.";
            }
            
            return RedirectToAction(nameof(UserDetails), new { id });
        }

        public async Task<IActionResult> ChangePassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }
            
            var viewModel = new ChangeUserPasswordViewModel
            {
                UserId = user.Id,
                UserName = user.UserName
            };
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangeUserPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
                return View(model);
            }
            
            var result = await _adminService.ChangeUserPasswordAsync(model.UserId, model.NewPassword);
            
            if (result)
            {
                TempData["StatusMessage"] = "Password has been changed successfully.";
                return RedirectToAction(nameof(UserDetails), new { id = model.UserId });
            }
            
            ModelState.AddModelError("", "Failed to change password.");
            return View(model);
        }

        public async Task<IActionResult> ContentModeration(int page = 1)
        {
            const int pageSize = 10;
            var posts = await _adminService.GetFlaggedPostsAsync(page, pageSize);
            
            ViewBag.CurrentPage = page;
            
            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModeratePost(int postId, ModerationAction action, string reason)
        {
            var result = await _adminService.ModeratePostAsync(postId, action, reason);
            
            if (result)
            {
                TempData["StatusMessage"] = $"Post has been {action.ToString().ToLower()}ed.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to moderate post.";
            }
            
            return RedirectToAction(nameof(ContentModeration));
        }

        public async Task<IActionResult> ManageRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }
            
            var currentRoles = await _adminService.GetUserRolesAsync(id);
            var allRoles = await _adminService.GetAllRolesAsync();
            
            var viewModel = new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                CurrentRoles = currentRoles,
                AvailableRoles = allRoles.Except(currentRoles).ToList()
            };
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToRole(UserRoleViewModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedRole))
            {
                TempData["ErrorMessage"] = "Please select a role.";
                return RedirectToAction(nameof(ManageRoles), new { id = model.UserId });
            }
            
            var result = await _adminService.AssignRoleToUserAsync(model.UserId, model.SelectedRole);
            
            if (result)
            {
                TempData["StatusMessage"] = $"User has been added to the {model.SelectedRole} role.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add user to role.";
            }
            
            return RedirectToAction(nameof(ManageRoles), new { id = model.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromRole(string userId, string roleName)
        {
            var result = await _adminService.RemoveRoleFromUserAsync(userId, roleName);
            
            if (result)
            {
                TempData["StatusMessage"] = $"User has been removed from the {roleName} role.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove user from role.";
            }
            
            return RedirectToAction(nameof(ManageRoles), new { id = userId });
        }
    }
}

