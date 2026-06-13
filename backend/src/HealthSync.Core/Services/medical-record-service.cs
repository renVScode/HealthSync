using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs.MedicalRecord;
using HealthSync.Core.Entities;
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

    public async Task<List<MedicalRecordResponseDto>> GetByPatientIdAsync(Guid patientId)
    {
        var records = await _uow.MedicalRecords.Query()
            .Include(r => r.Patient)
            .Include(r => r.Doctor)
            .Include(r => r.Prescriptions).ThenInclude(p => p.Medicine)
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

    public async Task<MedicalRecordResponseDto?> UpdateAsync(Guid id, UpdateMedicalRecordDto dto)
    {
        var record = await _uow.MedicalRecords.GetByIdAsync(id);
        if (record == null) return null;

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
            .Where(p => p.MedicalRecordId == medicalRecordId)
            .ToListAsync();

        return prescriptions.Select(p => new PrescriptionResponseDto
        {
            Id = p.Id,
            MedicineId = p.MedicineId,
            MedicineName = p.Medicine.Name,
            Dosage = p.Dosage,
            Frequency = p.Frequency,
            Duration = p.Duration,
            Instructions = p.Instructions,
            Quantity = p.Quantity,
            IsDispensed = p.IsDispensed
        }).ToList();
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
            Quantity = dto.Quantity
        };

        await _uow.Prescriptions.AddAsync(prescription);
        await _uow.SaveChangesAsync();
        return (await GetPrescriptionsAsync(medicalRecordId)).Last();
    }

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
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Prescriptions = r.Prescriptions.Select(p => new PrescriptionResponseDto
        {
            Id = p.Id,
            MedicineId = p.MedicineId,
            MedicineName = p.Medicine.Name,
            Dosage = p.Dosage,
            Frequency = p.Frequency,
            Duration = p.Duration,
            Instructions = p.Instructions,
            Quantity = p.Quantity,
            IsDispensed = p.IsDispensed
        }).ToList()
    };
}
