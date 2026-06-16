using HealthSync.Core.Entities;
using HealthSync.Core.Entities.Identity;

namespace HealthSync.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<ApplicationUser> Users { get; }
    IRepository<Patient> Patients { get; }


    IRepository<Doctor> Doctors { get; }
    IRepository<DoctorAvailability> DoctorAvailabilities { get; }
    IRepository<TimeOff> TimeOffs { get; }
    IRepository<Appointment> Appointments { get; }
    IRepository<MedicalRecord> MedicalRecords { get; }
    IRepository<Prescription> Prescriptions { get; }
    IRepository<Medicine> Medicines { get; }
    IRepository<InventoryBatch> InventoryBatches { get; }
    IRepository<InventoryTransaction> InventoryTransactions { get; }
    IRepository<Billing> Billings { get; }
    IRepository<BillingItem> BillingItems { get; }
    IRepository<Payment> Payments { get; }
    IRepository<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
