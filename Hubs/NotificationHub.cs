using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SocialMediaApp.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Add user to a group with their user ID for targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Remove user from their group when disconnected
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            await base.OnDisconnectedAsync(exception);
        }
    }
}

