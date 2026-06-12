namespace HealthSync.Core.Entities;

public class TimeOff
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }
    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // navigation
    public Doctor Doctor { get; set; } = null!;
}
