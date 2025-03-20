using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Hubs;
using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<PostHub> _postHubContext;

        public PostService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IHubContext<PostHub> postHubContext)
        {
            _context = context;
            _environment = environment;
            _postHubContext = postHubContext;
        }

        public async Task<List<PostViewModel>> GetFeedPostsAsync(string userId)
        {
            // Get IDs of friends
            var friendIds = await _context.Friendships
                .Where(f => f.UserId == userId)
                .Select(f => f.FriendId)
                .ToListAsync();

            // Add current user to see their posts too
            friendIds.Add(userId);

            // Get posts from friends and current user, ordered by creation date
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .Where(p => friendIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return posts.Select(p => MapPostToViewModel(p, userId)).ToList();
        }

        public async Task<List<PostViewModel>> GetUserPostsAsync(string userId)
        {
            var currentUserId = userId; // For checking if current user liked posts

            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return posts.Select(p => MapPostToViewModel(p, currentUserId)).ToList();
        }

        public async Task<PostViewModel> GetPostWithDetailsAsync(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                return null;
            }

            return MapPostToViewModel(post, null);
        }

        public async Task<Post> GetPostAsync(int postId)
        {
            return await _context.Posts.FindAsync(postId);
        }

        public async Task<int> CreatePostAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post.Id;
        }

        public async Task<string> UploadImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/uploads/" + uniqueFileName;
        }

        public async Task AddCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Get updated comment count
            var commentCount = await _context.Comments.CountAsync(c => c.PostId == comment.PostId);

            // Send real-time update for comment count
            await _postHubContext.Clients.Group($"post_{comment.PostId}").SendAsync("UpdateCommentCount", comment.PostId, commentCount);

            // Get the comment with user details for real-time display
            var commentWithUser = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            if (commentWithUser != null)
            {
                var commentViewModel = new CommentViewModel
                {
                    Id = commentWithUser.Id,
                    Content = commentWithUser.Content,
                    CreatedAt = commentWithUser.CreatedAt,
                    PostId = commentWithUser.PostId,
                    User = new UserViewModel
                    {
                        Id = commentWithUser.User.Id,
                        UserName = commentWithUser.User.UserName,
                        FirstName = commentWithUser.User.FirstName,
                        LastName = commentWithUser.User.LastName,
                        FullName = $"{commentWithUser.User.FirstName} {commentWithUser.User.LastName}",
                        ProfilePictureUrl = commentWithUser.User.ProfilePictureUrl
                    }
                };

                // Send the new comment for real-time display
                await _postHubContext.Clients.Group($"post_{comment.PostId}").SendAsync("NewComment", commentViewModel);
            }
        }

        public async Task<(bool WasAdded, int LikeCount)> ToggleLikeAsync(int postId, string userId)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            bool wasAdded = false;

            if (existingLike != null)
            {
                // Unlike
                _context.Likes.Remove(existingLike);
            }
            else
            {
                // Like
                _context.Likes.Add(new Like
                {
                    PostId = postId,
                    UserId = userId
                });
                wasAdded = true;
            }

            await _context.SaveChangesAsync();

            // Get updated like count
            var likeCount = await _context.Likes.CountAsync(l => l.PostId == postId);

            // Send real-time update for like count
            await _postHubContext.Clients.Group($"post_{postId}").SendAsync("UpdateLikeCount", postId, likeCount, wasAdded);

            return (wasAdded, likeCount);
        }

        public async Task DeletePostAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                // Notify clients that the post has been deleted
                await _postHubContext.Clients.All.SendAsync("PostDeleted", postId);
            }
        }

        private PostViewModel MapPostToViewModel(Post post, string currentUserId)
        {
            return new PostViewModel
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                User = new UserViewModel
                {
                    Id = post.User.Id,
                    UserName = post.User.UserName,
                    FullName = $"{post.User.FirstName} {post.User.LastName}",
                    ProfilePictureUrl = post.User.ProfilePictureUrl
                },
                Comments = post.Comments
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CommentViewModel
                    {
                        Id = c.Id,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        User = new UserViewModel
                        {
                            Id = c.User.Id,
                            UserName = c.User.UserName,
                            FullName = $"{c.User.FirstName} {c.User.LastName}",
                            ProfilePictureUrl = c.User.ProfilePictureUrl
                        }
                    }).ToList(),
                LikeCount = post.Likes.Count,
                IsLikedByCurrentUser = currentUserId != null && post.Likes.Any(l => l.UserId == currentUserId)
            };
        }
    }
}

