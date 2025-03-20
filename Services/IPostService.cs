using Microsoft.AspNetCore.Http;
using SocialMediaApp.Models;
using SocialMediaApp.ViewModels;

namespace SocialMediaApp.Services
{
    public interface IPostService
    {
        Task<List<PostViewModel>> GetFeedPostsAsync(string userId);
        Task<List<PostViewModel>> GetUserPostsAsync(string userId);
        Task<PostViewModel> GetPostWithDetailsAsync(int postId);
        Task<Post> GetPostAsync(int postId);
        Task<int> CreatePostAsync(Post post);
        Task<string> UploadImageAsync(IFormFile image);
        Task AddCommentAsync(Comment comment);
        Task<(bool WasAdded, int LikeCount)> ToggleLikeAsync(int postId, string userId);
        Task DeletePostAsync(int postId);
    }
}

