using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.MedicalRecord;
using HealthSync.Core.Enums;

namespace HealthSync.Core.Interfaces.Services;

public interface IMedicalRecordService
{
    Task<PaginatedResult<MedicalRecordResponseDto>> GetAllAsync(int page, int pageSize, bool? isArchived = null);
    Task<List<MedicalRecordResponseDto>> GetByPatientIdAsync(Guid patientId);
    Task<MedicalRecordResponseDto?> GetByIdAsync(Guid id);
    Task<MedicalRecordResponseDto> CreateAsync(CreateMedicalRecordDto dto, string userId);
    Task<MedicalRecordResponseDto?> UpdateAsync(Guid id, UpdateMedicalRecordDto dto);
    Task<List<PrescriptionResponseDto>> GetPrescriptionsAsync(Guid medicalRecordId);
    Task<PrescriptionResponseDto> AddPrescriptionAsync(Guid medicalRecordId, CreatePrescriptionDto dto);
    Task<List<PrescriptionResponseDto>> AddPrescriptionsAsync(Guid medicalRecordId, List<CreatePrescriptionDto> dtos);
    Task<List<PrescriptionResponseDto>> GetPrescriptionsByStatusAsync(PrescriptionStatus status, int page = 1, int pageSize = 25);
    Task<bool> MarkPrescriptionsAsPaidAsync(Guid medicalRecordId);
    Task<bool> DispensePrescriptionAsync(Guid prescriptionId, DispensePrescriptionDto dto, string userId);
    Task<bool> ArchiveAsync(Guid id);
    Task<bool> RestoreAsync(Guid id);
}
