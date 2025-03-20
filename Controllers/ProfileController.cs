using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Services;
using SocialMediaApp.ViewModels;
using System.Drawing;
using System.Security.Claims;

namespace SocialMediaApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IPostService _postService;
        private readonly IFriendService _friendService;

        public ProfileController(
            IPostService postService,
            IFriendService friendService)
        {
            _postService = postService;
            _friendService = friendService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"Current user ID: {currentUserId}", Color.Red);

            // If no id provided, show current user's profile
            if (string.IsNullOrEmpty(id))
            {
                id = currentUserId;
            }

            var user = await _friendService.GetUserProfileAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var posts = await _postService.GetUserPostsAsync(id);
            var friendStatus = await _friendService.GetFriendStatusAsync(currentUserId, id);

            var viewModel = new ProfileViewModel
            {
                User = user,
                Posts = posts,
                FriendStatus = friendStatus,
                IsCurrentUser = id == currentUserId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendFriendRequest(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _friendService.SendFriendRequestAsync(currentUserId, userId);

            return RedirectToAction("Index", new { id = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptFriendRequest(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _friendService.AcceptFriendRequestAsync(userId, currentUserId);

            return RedirectToAction("Index", new { id = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectFriendRequest(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _friendService.RejectFriendRequestAsync(userId, currentUserId);

            return RedirectToAction("Index", new { id = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFriend(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _friendService.RemoveFriendAsync(currentUserId, userId);

            return RedirectToAction("Index", new { id = userId });
        }
    }
}

