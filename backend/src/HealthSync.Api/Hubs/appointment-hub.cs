using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace HealthSync.Api.Hubs;

[Authorize]
public class AppointmentHub : Hub
{
    public async Task JoinDoctorGroup(string doctorId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"doctor_{doctorId}");
    }

    public async Task LeaveDoctorGroup(string doctorId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"doctor_{doctorId}");
    }

    public async Task JoinDateGroup(string date)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"date_{date}");
    }

    public async Task LeaveDateGroup(string date)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"date_{date}");
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
