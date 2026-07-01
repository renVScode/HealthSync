namespace HealthSync.Core.Entities;

public class DoctorServiceOffering
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // navigation
    public Doctor Doctor { get; set; } = null!;
}
