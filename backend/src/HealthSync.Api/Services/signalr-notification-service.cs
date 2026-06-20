using HealthSync.Api.Hubs;
using HealthSync.Core.DTOs.Appointment;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace HealthSync.Api.Services;

public class SignalRNotificationService : IAppointmentNotificationService
{
    private readonly IHubContext<AppointmentHub> _hubContext;

    public SignalRNotificationService(IHubContext<AppointmentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyAppointmentCreated(AppointmentResponseDto dto)
        => await _hubContext.Clients.All.SendAsync("AppointmentCreated", dto);

    public async Task NotifyAppointmentUpdated(AppointmentResponseDto dto)
        => await _hubContext.Clients.All.SendAsync("AppointmentUpdated", dto);

    public async Task NotifyAppointmentStatusChanged(Guid id, AppointmentStatus status)
        => await _hubContext.Clients.All.SendAsync("AppointmentStatusChanged", new { id, status });

    public async Task NotifyAppointmentCancelled(Guid id)
        => await _hubContext.Clients.All.SendAsync("AppointmentCancelled", id);
}
