using HealthSync.Core.Entities;
using HealthSync.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<TimeOff> TimeOffs => Set<TimeOff>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<InventoryBatch> InventoryBatches => Set<InventoryBatch>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Billing> Billings => Set<Billing>();
    public DbSet<BillingItem> BillingItems => Set<BillingItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<DoctorServiceOffering> DoctorServiceOfferings => Set<DoctorServiceOffering>();
    public DbSet<LabTest> LabTests => Set<LabTest>();
    public DbSet<LabOrder> LabOrders => Set<LabOrder>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.UserName).HasMaxLength(256).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.PasswordHash);
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(u => u.RefreshToken).HasMaxLength(500);
            entity.HasIndex(u => u.UserName).IsUnique();
            entity.HasIndex(u => u.Role);
            entity.HasIndex(u => u.IsActive);
            entity.HasIndex(u => u.IsArchived);
            entity.HasIndex(u => u.Email);
        });

        // Patient
        builder.Entity<Patient>(entity =>
        {
            entity.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Gender).HasMaxLength(10).IsRequired();
            entity.Property(p => p.Phone).HasMaxLength(20).IsRequired();
            entity.Property(p => p.Email).HasMaxLength(255);
            entity.Property(p => p.BloodType).HasMaxLength(5);
            entity.Property(p => p.EmergencyContact).HasMaxLength(100);
            entity.Property(p => p.EmergencyPhone).HasMaxLength(20);
            entity.HasOne(p => p.User).WithOne(u => u.Patient).HasForeignKey<Patient>(p => p.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(p => new { p.LastName, p.FirstName });
        });

        // Doctor
        builder.Entity<Doctor>(entity =>
        {
            entity.Property(d => d.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(d => d.LastName).HasMaxLength(100).IsRequired();
            entity.Property(d => d.Specialization).HasMaxLength(200).IsRequired();
            entity.Property(d => d.LicenseNumber).HasMaxLength(50).IsRequired();
            entity.Property(d => d.ProfileImageUrl).HasMaxLength(500);
            entity.Property(d => d.LicenseImageUrl).HasMaxLength(500);
            entity.HasIndex(d => d.LicenseNumber).IsUnique();
            entity.HasOne(d => d.User).WithOne(u => u.Doctor).HasForeignKey<Doctor>(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // DoctorAvailability
        builder.Entity<DoctorAvailability>(entity =>
        {
            entity.HasOne(a => a.Doctor).WithMany(d => d.Availabilities).HasForeignKey(a => a.DoctorId);
            entity.HasIndex(a => new { a.DoctorId, a.DayOfWeek, a.StartTime }).IsUnique();
        });

        // TimeOff
        builder.Entity<TimeOff>(entity =>
        {
            entity.HasOne(t => t.Doctor).WithMany(d => d.TimeOffs).HasForeignKey(t => t.DoctorId);
        });

        // Appointment
        builder.Entity<Appointment>(entity =>
        {
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(a => a.Reason).HasMaxLength(500);
            entity.Property(a => a.CancellationReason).HasMaxLength(500);
            entity.HasOne(a => a.Patient).WithMany(p => p.Appointments).HasForeignKey(a => a.PatientId);
            entity.HasOne(a => a.Doctor).WithMany(d => d.Appointments).HasForeignKey(a => a.DoctorId);
            entity.HasIndex(a => new { a.DoctorId, a.StartTime });
            entity.HasIndex(a => a.PatientId);
        });

        // MedicalRecord
        builder.Entity<MedicalRecord>(entity =>
        {
            entity.HasOne(r => r.Patient).WithMany(p => p.MedicalRecords).HasForeignKey(r => r.PatientId);
            entity.HasOne(r => r.Doctor).WithMany(d => d.MedicalRecords).HasForeignKey(r => r.DoctorId);
            entity.HasOne(r => r.Appointment).WithOne(a => a.MedicalRecord).HasForeignKey<MedicalRecord>(r => r.AppointmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(r => r.PatientId);
        });

        // Prescription
        builder.Entity<Prescription>(entity =>
        {
            entity.Property(p => p.Dosage).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Frequency).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Duration).HasMaxLength(100);
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasOne(p => p.MedicalRecord).WithMany(r => r.Prescriptions).HasForeignKey(p => p.MedicalRecordId);
            entity.HasOne(p => p.Medicine).WithMany(m => m.Prescriptions).HasForeignKey(p => p.MedicineId);
            entity.HasOne(p => p.DispensedByUser).WithMany().HasForeignKey(p => p.DispensedByUserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.InventoryBatch).WithMany().HasForeignKey(p => p.InventoryBatchId).OnDelete(DeleteBehavior.SetNull);
        });

        // Medicine
        builder.Entity<Medicine>(entity =>
        {
            entity.Property(m => m.Name).HasMaxLength(200).IsRequired();
            entity.Property(m => m.GenericName).HasMaxLength(200);
            entity.Property(m => m.Category).HasMaxLength(100);
            entity.Property(m => m.Manufacturer).HasMaxLength(200);
            entity.Property(m => m.Unit).HasMaxLength(50).IsRequired();
            entity.Property(m => m.Price).HasColumnType("decimal(10,2)");
            entity.HasIndex(m => m.Name);
        });

        // InventoryBatch
        builder.Entity<InventoryBatch>(entity =>
        {
            entity.Property(b => b.BatchNumber).HasMaxLength(100).IsRequired();
            entity.Property(b => b.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(b => b.Supplier).HasMaxLength(200);
            entity.Property(b => b.Location).HasMaxLength(100);
            entity.HasOne(b => b.Medicine).WithMany(m => m.InventoryBatches).HasForeignKey(b => b.MedicineId);
            entity.HasIndex(b => b.MedicineId);
            entity.HasIndex(b => b.ExpiryDate);
        });

        // InventoryTransaction
        builder.Entity<InventoryTransaction>(entity =>
        {
            entity.Property(t => t.TransactionType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(t => t.ReferenceType).HasMaxLength(50);
            entity.HasOne(t => t.InventoryBatch).WithMany(b => b.Transactions).HasForeignKey(t => t.InventoryBatchId);
            entity.HasOne(t => t.CreatedBy).WithMany(u => u.InventoryTransactions).HasForeignKey(t => t.CreatedById).OnDelete(DeleteBehavior.NoAction);
        });

        // Billing
        builder.Entity<Billing>(entity =>
        {
            entity.Property(b => b.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(b => b.SubTotal).HasColumnType("decimal(12,2)");
            entity.Property(b => b.Discount).HasColumnType("decimal(12,2)");
            entity.Property(b => b.Tax).HasColumnType("decimal(12,2)");
            entity.Property(b => b.Total).HasColumnType("decimal(12,2)");
            entity.Property(b => b.AmountPaid).HasColumnType("decimal(12,2)");
            entity.Property(b => b.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasOne(b => b.Patient).WithMany(p => p.Billings).HasForeignKey(b => b.PatientId);
            entity.HasOne(b => b.Appointment).WithOne(a => a.Billing).HasForeignKey<Billing>(b => b.AppointmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(b => b.CreatedBy).WithMany().HasForeignKey(b => b.CreatedById).OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(b => b.InvoiceNumber).IsUnique();
            entity.HasIndex(b => b.PatientId);
        });

        // BillingItem
        builder.Entity<BillingItem>(entity =>
        {
            entity.Property(i => i.Description).HasMaxLength(500).IsRequired();
            entity.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(i => i.Total).HasColumnType("decimal(12,2)");
            entity.HasOne(i => i.Billing).WithMany(b => b.Items).HasForeignKey(i => i.BillingId);
        });

        // Payment
        builder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasColumnType("decimal(12,2)");
            entity.Property(p => p.PaymentMethod).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(p => p.TransactionReference).HasMaxLength(255);
            entity.Property(p => p.QrCodeImageUrl).HasMaxLength(500);
            entity.Property(p => p.PaymentDetails);
            entity.HasOne(p => p.Billing).WithMany(b => b.Payments).HasForeignKey(p => p.BillingId);
            entity.HasOne(p => p.ReceivedBy).WithMany().HasForeignKey(p => p.ReceivedById).OnDelete(DeleteBehavior.NoAction);
        });

        // LabTest
        builder.Entity<LabTest>(entity =>
        {
            entity.Property(t => t.TestName).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Category).HasMaxLength(100);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.Price).HasColumnType("decimal(10,2)");
            entity.HasIndex(t => t.TestName).IsUnique();
        });

        // LabOrder
        builder.Entity<LabOrder>(entity =>
        {
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(o => o.Result).HasColumnType("text");
            entity.Property(o => o.ResultSummary).HasMaxLength(500);
            entity.Property(o => o.Notes).HasMaxLength(500);
            entity.Property(o => o.ReferenceRange).HasMaxLength(200);
            entity.HasOne(o => o.Patient).WithMany().HasForeignKey(o => o.PatientId);
            entity.HasOne(o => o.Doctor).WithMany().HasForeignKey(o => o.DoctorId);
            entity.HasOne(o => o.LabTest).WithMany(t => t.LabOrders).HasForeignKey(o => o.LabTestId);
            entity.HasIndex(o => new { o.PatientId, o.Status });
            entity.HasIndex(o => o.DoctorId);
        });

        // DoctorServiceOffering
        builder.Entity<DoctorServiceOffering>(entity =>
        {
            entity.Property(s => s.ServiceName).HasMaxLength(200).IsRequired();
            entity.Property(s => s.Description).HasMaxLength(500);
            entity.Property(s => s.Price).HasColumnType("decimal(10,2)");
            entity.HasOne(s => s.Doctor).WithMany().HasForeignKey(s => s.DoctorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(s => new { s.DoctorId, s.ServiceName }).IsUnique();
        });

        // AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.Property(l => l.Action).HasMaxLength(100).IsRequired();
            entity.Property(l => l.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(l => l.IpAddress).HasMaxLength(50);
            entity.HasOne(l => l.User).WithMany(u => u.AuditLogs).HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(l => new { l.EntityType, l.EntityId });
        });
    }
}
