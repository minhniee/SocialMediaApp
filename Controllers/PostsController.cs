using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Models;
using SocialMediaApp.Services;
using SocialMediaApp.ViewModels;
using System.Security.Claims;

namespace SocialMediaApp.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;

        public PostsController(
            IPostService postService,
            INotificationService notificationService)
        {
            _postService = postService;
            _notificationService = notificationService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HomeViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    //ModelState.AddModelError("NewPost.Content", "cdi);k
            //    return RedirectToAction("Index", "Home");
            //}

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var post = new Post
            {
                Content = model.NewPost.Content,
                UserId = userId
            };

            // Handle image upload if present
            if (model.NewPost.Image != null)
            {
                var imageUrl = await _postService.UploadImageAsync(model.NewPost.Image);
                post.ImageUrl = imageUrl;
            }

            await _postService.CreatePostAsync(post);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(string PostId, string Content)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new Comment
            {
                Content = Content,
                PostId = Int32.Parse(PostId),
                UserId = userId
            };

            await _postService.AddCommentAsync(comment);

            // Create notification for post owner
            await _notificationService.CreateCommentNotificationAsync(comment);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _postService.ToggleLikeAsync(postId, userId);

            if (result.WasAdded)
            {
                // Create notification for post owner
                await _notificationService.CreateLikeNotificationAsync(postId, userId);
            }

            return Json(new { success = true, liked = result.WasAdded, likeCount = result.LikeCount });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _postService.GetPostWithDetailsAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await _postService.GetPostAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            if (post.UserId != userId)
            {
                return Forbid();
            }

            await _postService.DeletePostAsync(id);

            return RedirectToAction("Index", "Home");
        }
    }
}

