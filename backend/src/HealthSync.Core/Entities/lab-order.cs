using HealthSync.Core.Enums;

namespace HealthSync.Core.Entities;

public class LabOrder
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid LabTestId { get; set; }
    public LabOrderStatus Status { get; set; } = LabOrderStatus.Ordered;
    public string? Result { get; set; }
    public string? ResultSummary { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceRange { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // navigation
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public LabTest LabTest { get; set; } = null!;
}
