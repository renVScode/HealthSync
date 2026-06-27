using HealthSync.Core.Entities.Identity;
using HealthSync.Core.Enums;

namespace HealthSync.Core.Entities;

public class Prescription
{
    public Guid Id { get; set; }
    public Guid MedicalRecordId { get; set; }
    public Guid MedicineId { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public string? Instructions { get; set; }
    public int Quantity { get; set; }
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;
    public Guid? DispensedByUserId { get; set; }
    public DateTime? DispensedAt { get; set; }
    public Guid? InventoryBatchId { get; set; }

    // navigation
    public MedicalRecord MedicalRecord { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
    public ApplicationUser? DispensedByUser { get; set; }
    public InventoryBatch? InventoryBatch { get; set; }
}
