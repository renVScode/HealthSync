using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.Appointment;
using HealthSync.Core.Enums;

namespace HealthSync.Core.Interfaces.Services;

public interface IAppointmentService
{
    Task<PaginatedResult<AppointmentResponseDto>> GetAllAsync(AppointmentFilterDto filter);
    Task<AppointmentResponseDto?> GetByIdAsync(Guid id);
    Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto, string userId);
    Task<AppointmentResponseDto?> UpdateAsync(Guid id, UpdateAppointmentDto dto);
    Task<bool> UpdateStatusAsync(Guid id, AppointmentStatus status, string userId);
    Task<bool> CancelAsync(Guid id);
    Task<List<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end, Guid? doctorId);
}
