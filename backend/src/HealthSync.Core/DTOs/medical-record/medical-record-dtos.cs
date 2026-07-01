namespace HealthSync.Core.DTOs.MedicalRecord;

public class CreateMedicalRecordDto
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }
    public bool IsConfidential { get; set; }
}

public class UpdateMedicalRecordDto
{
    public string? Diagnosis { get; set; }
    public string? Symptoms { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }
    public bool? IsConfidential { get; set; }
}

public class MedicalRecordResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }
    public bool IsConfidential { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsArchived { get; set; }
    public bool IsCompleted { get; set; }
    public List<PrescriptionResponseDto> Prescriptions { get; set; } = [];
}

public class CreatePrescriptionDto
{
    public Guid MedicineId { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public string? Instructions { get; set; }
    public int Quantity { get; set; }
}

public class PrescriptionResponseDto
{
    public Guid Id { get; set; }
    public Guid MedicalRecordId { get; set; }
    public Guid MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public string? Instructions { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = "Pending";
    public string? DispensedBy { get; set; }
    public DateTime? DispensedAt { get; set; }
    public Guid? InventoryBatchId { get; set; }
}

public class DispensePrescriptionDto
{
    public Guid BatchId { get; set; }
    public int Quantity { get; set; }
}
