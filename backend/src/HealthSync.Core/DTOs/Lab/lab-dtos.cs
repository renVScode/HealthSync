using HealthSync.Core.Enums;

namespace HealthSync.Core.DTOs.Lab;

public class CreateLabTestDto
{
    public string TestName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

public class UpdateLabTestDto
{
    public string? TestName { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public bool? IsActive { get; set; }
}

public class LabTestResponseDto
{
    public Guid Id { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLabOrderDto
{
    public Guid PatientId { get; set; }
    public Guid LabTestId { get; set; }
    public Guid? DoctorId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLabOrderDto
{
    public LabOrderStatus? Status { get; set; }
    public string? Result { get; set; }
    public string? ResultSummary { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceRange { get; set; }
}

public class LabOrderResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid LabTestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public LabOrderStatus Status { get; set; }
    public string? Result { get; set; }
    public string? ResultSummary { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceRange { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime OrderedAt { get; set; }
}
