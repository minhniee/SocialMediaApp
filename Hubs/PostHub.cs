using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SocialMediaApp.Hubs
{
	public class PostHub : Hub
	{
		[Authorize]
		public async Task SendLike(int postId, int likeCount)
		{
			await Clients.Group($"post_{postId}").SendAsync("ReceiveLike", postId, likeCount);
		}
		[Authorize]
		public async Task SendComment(int postId, string commentHtml)
		{
			await Clients.Group($"post_{postId}").SendAsync("ReceiveComment", postId, commentHtml);
		}
		[Authorize]
		public async Task JoinPostGroup(int postId)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, $"post_{postId}");
		}
	}
}