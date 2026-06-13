using HealthSync.Core.DTOs.Appointment;
using HealthSync.Core.Enums;

namespace HealthSync.Core.Interfaces.Services;

public interface IAppointmentNotificationService
{
    Task NotifyAppointmentCreated(AppointmentResponseDto dto);
    Task NotifyAppointmentUpdated(AppointmentResponseDto dto);
    Task NotifyAppointmentStatusChanged(Guid id, AppointmentStatus status);
    Task NotifyAppointmentCancelled(Guid id);
}
