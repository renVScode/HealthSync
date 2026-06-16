using HealthSync.Core.Entities;
using HealthSync.Core.Entities.Identity;
using HealthSync.Core.Interfaces;
using HealthSync.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace HealthSync.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    private IRepository<ApplicationUser>? _users;
    private IRepository<Patient>? _patients;

    public IRepository<ApplicationUser> Users => _users ??= new GenericRepository<ApplicationUser>(_context);
    private IRepository<Doctor>? _doctors;
    private IRepository<DoctorAvailability>? _doctorAvailabilities;
    private IRepository<TimeOff>? _timeOffs;
    private IRepository<Appointment>? _appointments;
    private IRepository<MedicalRecord>? _medicalRecords;
    private IRepository<Prescription>? _prescriptions;
    private IRepository<Medicine>? _medicines;
    private IRepository<InventoryBatch>? _inventoryBatches;
    private IRepository<InventoryTransaction>? _inventoryTransactions;
    private IRepository<Billing>? _billings;
    private IRepository<BillingItem>? _billingItems;
    private IRepository<Payment>? _payments;
    private IRepository<AuditLog>? _auditLogs;

    public IRepository<Patient> Patients => _patients ??= new GenericRepository<Patient>(_context);
    public IRepository<Doctor> Doctors => _doctors ??= new GenericRepository<Doctor>(_context);
    public IRepository<DoctorAvailability> DoctorAvailabilities => _doctorAvailabilities ??= new GenericRepository<DoctorAvailability>(_context);
    public IRepository<TimeOff> TimeOffs => _timeOffs ??= new GenericRepository<TimeOff>(_context);
    public IRepository<Appointment> Appointments => _appointments ??= new GenericRepository<Appointment>(_context);
    public IRepository<MedicalRecord> MedicalRecords => _medicalRecords ??= new GenericRepository<MedicalRecord>(_context);
    public IRepository<Prescription> Prescriptions => _prescriptions ??= new GenericRepository<Prescription>(_context);
    public IRepository<Medicine> Medicines => _medicines ??= new GenericRepository<Medicine>(_context);
    public IRepository<InventoryBatch> InventoryBatches => _inventoryBatches ??= new GenericRepository<InventoryBatch>(_context);
    public IRepository<InventoryTransaction> InventoryTransactions => _inventoryTransactions ??= new GenericRepository<InventoryTransaction>(_context);
    public IRepository<Billing> Billings => _billings ??= new GenericRepository<Billing>(_context);
    public IRepository<BillingItem> BillingItems => _billingItems ??= new GenericRepository<BillingItem>(_context);
    public IRepository<Payment> Payments => _payments ??= new GenericRepository<Payment>(_context);
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new GenericRepository<AuditLog>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null) await _transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null) await _transaction.RollbackAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
