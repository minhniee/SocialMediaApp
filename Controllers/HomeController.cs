using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Data;
using SocialMediaApp.Models;
using SocialMediaApp.Services;
using SocialMediaApp.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace SocialMediaApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IPostService _postService;
        private readonly IFriendService _friendService;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            IFriendService friendService,
            IPostService postService)
        {
            _friendService = friendService;
            _logger = logger;
            _context = context;
            _postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("Landing");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var feedPosts = await _postService.GetFeedPostsAsync(userId);

            var sugguestionFriend = await _friendService.GetFriendSuggestionsAsync(userId);

            var viewModel = new HomeViewModel
            {
                FriendModel = sugguestionFriend,
                Posts = feedPosts,
                NewPost = new PostViewModel()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

