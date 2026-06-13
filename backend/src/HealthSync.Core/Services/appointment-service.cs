using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs.Appointment;
using HealthSync.Core.Entities;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IAppointmentNotificationService _notification;

    public AppointmentService(IUnitOfWork uow, IAppointmentNotificationService notification)
    {
        _uow = uow;
        _notification = notification;
    }

    public async Task<PaginatedResult<AppointmentResponseDto>> GetAllAsync(AppointmentFilterDto filter)
    {
        var query = _uow.Appointments.Query()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsQueryable();

        if (filter.PatientId.HasValue) query = query.Where(a => a.PatientId == filter.PatientId);
        if (filter.DoctorId.HasValue) query = query.Where(a => a.DoctorId == filter.DoctorId);
        if (filter.Status.HasValue) query = query.Where(a => a.Status == filter.Status);
        if (filter.DateFrom.HasValue) query = query.Where(a => a.StartTime >= filter.DateFrom);
        if (filter.DateTo.HasValue) query = query.Where(a => a.StartTime <= filter.DateTo);

        var total = await query.CountAsync();
        var items = await query.OrderBy(a => a.StartTime)
                               .Skip((filter.Page - 1) * filter.PageSize)
                               .Take(filter.PageSize)
                               .ToListAsync();

        return new PaginatedResult<AppointmentResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<AppointmentResponseDto?> GetByIdAsync(Guid id)
    {
        var appointment = await _uow.Appointments.Query()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id);

        return appointment == null ? null : MapToDto(appointment);
    }

    public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto, string userId)
    {
        // Round to nearest 15-minute interval
        var startTime = RoundToNearest15(dto.StartTime);
        var endTime = startTime.AddMinutes(15);

        // Check for overlapping appointments
        var hasOverlap = await _uow.Appointments.ExistsAsync(a =>
            a.DoctorId == dto.DoctorId &&
            a.StartTime < endTime &&
            a.EndTime > startTime &&
            a.Status != AppointmentStatus.Cancelled);

        if (hasOverlap)
            throw new InvalidOperationException("Time slot is already booked");

        // Check doctor availability and time-offs
        var dayOfWeek = (int)startTime.DayOfWeek;
        var timeOnly = TimeOnly.FromDateTime(startTime);

        var isAvailable = await _uow.DoctorAvailabilities.ExistsAsync(a =>
            a.DoctorId == dto.DoctorId &&
            (int)a.DayOfWeek == dayOfWeek &&
            a.StartTime <= timeOnly &&
            a.EndTime > timeOnly &&
            a.IsAvailable);

        if (!isAvailable)
            throw new ArgumentException("Doctor is not available at this time");

        var dateOnly = DateOnly.FromDateTime(startTime);
        var hasTimeOff = await _uow.TimeOffs.ExistsAsync(t =>
            t.DoctorId == dto.DoctorId &&
            t.StartDate <= dateOnly &&
            t.EndDate >= dateOnly &&
            t.IsApproved);

        if (hasTimeOff)
            throw new ArgumentException("Doctor has time off during this period");

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            StartTime = startTime,
            EndTime = endTime,
            Reason = dto.Reason,
            Notes = dto.Notes,
            Status = AppointmentStatus.Scheduled
        };

        await _uow.Appointments.AddAsync(appointment);
        await _uow.SaveChangesAsync();

        // Broadcast via SignalR
        await _notification.NotifyAppointmentCreated(MapToDto(appointment));

        return MapToDto(appointment);
    }

    public async Task<AppointmentResponseDto?> UpdateAsync(Guid id, UpdateAppointmentDto dto)
    {
        var appointment = await _uow.Appointments.Query()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return null;

        if (dto.StartTime.HasValue)
        {
            var newStart = RoundToNearest15(dto.StartTime.Value);
            appointment.StartTime = newStart;
            appointment.EndTime = newStart.AddMinutes(15);
        }
        if (dto.Reason != null) appointment.Reason = dto.Reason;
        if (dto.Notes != null) appointment.Notes = dto.Notes;

        appointment.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        await _notification.NotifyAppointmentCreated(MapToDto(appointment));
        return MapToDto(appointment);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, AppointmentStatus status, string userId)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id);
        if (appointment == null) return false;

        if (!IsValidTransition(appointment.Status, status))
            return false;

        appointment.Status = status;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        await _notification.NotifyAppointmentStatusChanged(id, status);
        return true;
    }

    public async Task<bool> CancelAsync(Guid id)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id);
        if (appointment == null) return false;

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        await _notification.NotifyAppointmentCancelled(id);
        return true;
    }

    public async Task<List<CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end, Guid? doctorId)
    {
        var query = _uow.Appointments.Query()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.StartTime >= start && a.EndTime <= end);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId);

        var appointments = await query.ToListAsync();
        return appointments.Select(a => new CalendarEventDto
        {
            Id = a.Id,
            Title = $"{a.Patient.FirstName} {a.Patient.LastName}" + (a.Reason != null ? $" - {a.Reason}" : ""),
            Start = a.StartTime,
            End = a.EndTime,
            Status = a.Status,
            Color = GetStatusColor(a.Status),
            PatientId = a.PatientId,
            DoctorId = a.DoctorId
        }).ToList();
    }

    private static DateTime RoundToNearest15(DateTime dt)
    {
        var minutes = (dt.Minute / 15) * 15;
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, minutes, 0, dt.Kind);
    }

    private static bool IsValidTransition(AppointmentStatus current, AppointmentStatus next)
    {
        return (current, next) switch
        {
            (AppointmentStatus.Scheduled, AppointmentStatus.Confirmed) => true,
            (AppointmentStatus.Scheduled, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.InProgress) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.InProgress, AppointmentStatus.Completed) => true,
            (AppointmentStatus.InProgress, AppointmentStatus.Cancelled) => true,
            (AppointmentStatus.Confirmed, AppointmentStatus.NoShow) => true,
            _ => false
        };
    }

    private static string GetStatusColor(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Scheduled => "#17A2B8",
        AppointmentStatus.Confirmed => "#28A745",
        AppointmentStatus.InProgress => "#FFC107",
        AppointmentStatus.Completed => "#6C757D",
        AppointmentStatus.Cancelled => "#DC3545",
        AppointmentStatus.NoShow => "#DC3545",
        _ => "#ADB5BD"
    };

    private static AppointmentResponseDto MapToDto(Appointment a) => new()
    {
        Id = a.Id,
        PatientId = a.PatientId,
        PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
        DoctorId = a.DoctorId,
        DoctorName = $"{a.Doctor.FirstName} {a.Doctor.LastName}",
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = a.Status,
        Reason = a.Reason,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt
    };
}
