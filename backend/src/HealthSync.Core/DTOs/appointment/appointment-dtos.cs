using HealthSync.Core.Enums;

namespace HealthSync.Core.DTOs.Appointment;

public class CreateAppointmentDto
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime StartTime { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAppointmentDto
{
    public DateTime? StartTime { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAppointmentStatusDto
{
    public AppointmentStatus Status { get; set; }
    public string? CancellationReason { get; set; }
}

public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AppointmentFilterDto
{
    public Guid? PatientId { get; set; }
    public Guid? DoctorId { get; set; }
    public AppointmentStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CalendarEventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Color { get; set; }
    public AppointmentStatus Status { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
