namespace HealthSync.Core.Entities;

public class DoctorAvailability
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDuration { get; set; } = 15; // 15 mins interval
    public bool IsAvailable { get; set; } = true;

    // navigation
    public Doctor Doctor { get; set; } = null!;
}
