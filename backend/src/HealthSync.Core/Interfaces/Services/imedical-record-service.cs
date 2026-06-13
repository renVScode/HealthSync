using HealthSync.Core.DTOs.MedicalRecord;

namespace HealthSync.Core.Interfaces.Services;

public interface IMedicalRecordService
{
    Task<List<MedicalRecordResponseDto>> GetByPatientIdAsync(Guid patientId);
    Task<MedicalRecordResponseDto?> GetByIdAsync(Guid id);
    Task<MedicalRecordResponseDto> CreateAsync(CreateMedicalRecordDto dto, string userId);
    Task<MedicalRecordResponseDto?> UpdateAsync(Guid id, UpdateMedicalRecordDto dto);
    Task<List<PrescriptionResponseDto>> GetPrescriptionsAsync(Guid medicalRecordId);
    Task<PrescriptionResponseDto> AddPrescriptionAsync(Guid medicalRecordId, CreatePrescriptionDto dto);
}
