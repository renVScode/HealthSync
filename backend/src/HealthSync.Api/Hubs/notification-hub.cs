using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace HealthSync.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task JoinRoleGroup(string role)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role}");
    }

    public async Task LeaveRoleGroup(string role)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"role_{role}");
    }

    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role}");

        await base.OnConnectedAsync();
    }
}
