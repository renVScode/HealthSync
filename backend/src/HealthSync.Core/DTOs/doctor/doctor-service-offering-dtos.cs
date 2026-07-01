namespace HealthSync.Core.DTOs.Doctor;

public class CreateServiceOfferingDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

public class UpdateServiceOfferingDto
{
    public string? ServiceName { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public bool? IsActive { get; set; }
}

public class ServiceOfferingResponseDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
