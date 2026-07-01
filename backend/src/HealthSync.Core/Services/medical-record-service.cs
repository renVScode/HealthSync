using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.MedicalRecord;
using HealthSync.Core.Entities;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IUnitOfWork _uow;

    public MedicalRecordService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PaginatedResult<MedicalRecordResponseDto>> GetAllAsync(int page, int pageSize, bool? isArchived = null, string? status = null)
    {
        var query = _uow.MedicalRecords.Query()
            .Include(r => r.Patient)
            .Include(r => r.Doctor)
            .Include(r => r.Prescriptions).ThenInclude(p => p.Medicine)
            .Include(r => r.Prescriptions).ThenInclude(p => p.DispensedByUser)
            .AsQueryable();

        if (isArchived.HasValue)
            query = query.Where(r => r.IsArchived == isArchived.Value);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var isCompleted = status.Equals("completed", StringComparison.OrdinalIgnoreCase);
            query = query.Where(r => r.IsCompleted == isCompleted);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(r => r.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PaginatedResult<MedicalRecordResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<MedicalRecordResponseDto>> GetByPatientIdAsync(Guid patientId)
    {
        var records = await _uow.MedicalRecords.Query()
            .Include(r => r.Patient)
            .Include(r => r.Doctor)
            .Include(r => r.Prescriptions).ThenInclude(p => p.Medicine)
            .Include(r => r.Prescriptions).ThenInclude(p => p.DispensedByUser)
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return records.Select(MapToDto).ToList();
    }

    public async Task<MedicalRecordResponseDto?> GetByIdAsync(Guid id)
    {
        var record = await _uow.MedicalRecords.Query()
            .Include(r => r.Patient)
            .Include(r => r.Doctor)
            .Include(r => r.Prescriptions).ThenInclude(p => p.Medicine)
            .Include(r => r.Prescriptions).ThenInclude(p => p.DispensedByUser)
            .FirstOrDefaultAsync(r => r.Id == id);

        return record == null ? null : MapToDto(record);
    }

    public async Task<MedicalRecordResponseDto> CreateAsync(CreateMedicalRecordDto dto, string userId)
    {
        var doctor = await _uow.Doctors.FindAsync(d => d.UserId.ToString() == userId);
        var doctorId = doctor.FirstOrDefault()?.Id ?? Guid.Parse(userId);

        var record = new MedicalRecord
        {
            PatientId = dto.PatientId,
            DoctorId = doctorId,
            AppointmentId = dto.AppointmentId,
            Diagnosis = dto.Diagnosis,
            Symptoms = dto.Symptoms,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            IsConfidential = dto.IsConfidential
        };

        await _uow.MedicalRecords.AddAsync(record);
        await _uow.SaveChangesAsync();
        return (await GetByIdAsync(record.Id))!;
    }

    public async Task<List<PrescriptionResponseDto>> AddPrescriptionsAsync(Guid medicalRecordId, List<CreatePrescriptionDto> dtos)
    {
        var record = await _uow.MedicalRecords.GetByIdAsync(medicalRecordId);
        if (record == null) return [];

        foreach (var dto in dtos)
        {
            var prescription = new Prescription
            {
                MedicalRecordId = medicalRecordId,
                MedicineId = dto.MedicineId,
                Dosage = dto.Dosage,
                Frequency = dto.Frequency,
                Duration = dto.Duration,
                Instructions = dto.Instructions,
                Quantity = dto.Quantity,
                Status = PrescriptionStatus.Pending
            };
            await _uow.Prescriptions.AddAsync(prescription);
        }

        await _uow.SaveChangesAsync();
        return await GetPrescriptionsAsync(medicalRecordId);
    }

    public async Task<MedicalRecordResponseDto?> UpdateAsync(Guid id, UpdateMedicalRecordDto dto)
    {
        var record = await _uow.MedicalRecords.GetByIdAsync(id);
        if (record == null) return null;
        if (record.IsCompleted) return null;

        if (dto.Diagnosis != null) record.Diagnosis = dto.Diagnosis;
        if (dto.Symptoms != null) record.Symptoms = dto.Symptoms;
        if (dto.Treatment != null) record.Treatment = dto.Treatment;
        if (dto.Notes != null) record.Notes = dto.Notes;
        if (dto.IsConfidential.HasValue) record.IsConfidential = dto.IsConfidential.Value;

        record.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<List<PrescriptionResponseDto>> GetPrescriptionsAsync(Guid medicalRecordId)
    {
        var prescriptions = await _uow.Prescriptions.Query()
            .Include(p => p.Medicine)
            .Include(p => p.DispensedByUser)
            .Where(p => p.MedicalRecordId == medicalRecordId)
            .ToListAsync();

        return prescriptions.Select(MapPrescriptionToDto).ToList();
    }

    public async Task<PrescriptionResponseDto> AddPrescriptionAsync(Guid medicalRecordId, CreatePrescriptionDto dto)
    {
        var prescription = new Prescription
        {
            MedicalRecordId = medicalRecordId,
            MedicineId = dto.MedicineId,
            Dosage = dto.Dosage,
            Frequency = dto.Frequency,
            Duration = dto.Duration,
            Instructions = dto.Instructions,
            Quantity = dto.Quantity,
            Status = PrescriptionStatus.Pending
        };

        await _uow.Prescriptions.AddAsync(prescription);
        await _uow.SaveChangesAsync();
        return (await GetPrescriptionsAsync(medicalRecordId)).Last();
    }

    public async Task<List<PrescriptionResponseDto>> GetPrescriptionsByStatusAsync(PrescriptionStatus status, int page = 1, int pageSize = 25)
    {
        var query = _uow.Prescriptions.Query()
            .Include(p => p.Medicine)
            .Include(p => p.MedicalRecord).ThenInclude(r => r.Patient)
            .Include(p => p.MedicalRecord).ThenInclude(r => r.Doctor)
            .Include(p => p.DispensedByUser)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.MedicalRecord.CreatedAt);

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return items.Select(p =>
        {
            var dto = MapPrescriptionToDto(p);
            dto.PatientName = p.MedicalRecord?.Patient != null
                ? $"{p.MedicalRecord.Patient.FirstName} {p.MedicalRecord.Patient.LastName}"
                : null;
            return dto;
        }).ToList();
    }

    public async Task<bool> MarkPrescriptionsAsPaidAsync(Guid medicalRecordId)
    {
        var prescriptions = await _uow.Prescriptions.FindAsync(p => p.MedicalRecordId == medicalRecordId && p.Status == PrescriptionStatus.Pending);
        if (!prescriptions.Any()) return false;

        foreach (var p in prescriptions)
            p.Status = PrescriptionStatus.Paid;

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteAsync(Guid id)
    {
        var record = await _uow.MedicalRecords.GetByIdAsync(id);
        if (record == null || record.IsCompleted) return false;
        record.IsCompleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ArchiveAsync(Guid id)
    {
        var record = await _uow.MedicalRecords.GetByIdAsync(id);
        if (record == null) return false;
        record.IsArchived = true;
        record.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(Guid id)
    {
        var record = await _uow.MedicalRecords.GetByIdAsync(id);
        if (record == null) return false;
        record.IsArchived = false;
        record.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DispensePrescriptionAsync(Guid prescriptionId, DispensePrescriptionDto dto, string userId)
    {
        var prescription = await _uow.Prescriptions.Query()
            .FirstOrDefaultAsync(p => p.Id == prescriptionId && p.Status == PrescriptionStatus.Paid);

        if (prescription == null) return false;

        var batch = await _uow.InventoryBatches.GetByIdAsync(dto.BatchId);
        if (batch == null || batch.MedicineId != prescription.MedicineId || batch.Quantity < dto.Quantity)
            return false;

        batch.Quantity -= dto.Quantity;

        var transaction = new InventoryTransaction
        {
            InventoryBatchId = dto.BatchId,
            TransactionType = TransactionType.Dispensed,
            Quantity = dto.Quantity,
            ReferenceType = "Prescription",
            ReferenceId = prescriptionId,
            CreatedById = Guid.Parse(userId)
        };
        await _uow.InventoryTransactions.AddAsync(transaction);

        prescription.Status = PrescriptionStatus.Completed;
        prescription.DispensedByUserId = Guid.Parse(userId);
        prescription.DispensedAt = DateTime.UtcNow;
        prescription.InventoryBatchId = dto.BatchId;

        await _uow.SaveChangesAsync();

        var allCompleted = await _uow.Prescriptions.Query()
            .Where(p => p.MedicalRecordId == prescription.MedicalRecordId)
            .AllAsync(p => p.Status == PrescriptionStatus.Completed);

        if (allCompleted)
        {
            var record = await _uow.MedicalRecords.GetByIdAsync(prescription.MedicalRecordId);
            if (record != null)
            {
                record.IsCompleted = true;
                record.UpdatedAt = DateTime.UtcNow;
                await _uow.SaveChangesAsync();
            }
        }

        return true;
    }

    private static PrescriptionResponseDto MapPrescriptionToDto(Prescription p) => new()
    {
        Id = p.Id,
        MedicalRecordId = p.MedicalRecordId,
        MedicineId = p.MedicineId,
        MedicineName = p.Medicine.Name,
        Dosage = p.Dosage,
        Frequency = p.Frequency,
        Duration = p.Duration,
        Instructions = p.Instructions,
        Quantity = p.Quantity,
        Status = p.Status.ToString(),
        DispensedBy = p.DispensedByUser != null ? $"{p.DispensedByUser.FirstName} {p.DispensedByUser.LastName}" : null,
        DispensedAt = p.DispensedAt,
        InventoryBatchId = p.InventoryBatchId
    };

    private static MedicalRecordResponseDto MapToDto(MedicalRecord r) => new()
    {
        Id = r.Id,
        PatientId = r.PatientId,
        PatientName = $"{r.Patient.FirstName} {r.Patient.LastName}",
        DoctorId = r.DoctorId,
        DoctorName = $"{r.Doctor.FirstName} {r.Doctor.LastName}",
        AppointmentId = r.AppointmentId,
        Diagnosis = r.Diagnosis,
        Symptoms = r.Symptoms,
        Treatment = r.Treatment,
        Notes = r.Notes,
        IsConfidential = r.IsConfidential,
        IsArchived = r.IsArchived,
        IsCompleted = r.IsCompleted,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Prescriptions = r.Prescriptions.Select(MapPrescriptionToDto).ToList()
    };
}
