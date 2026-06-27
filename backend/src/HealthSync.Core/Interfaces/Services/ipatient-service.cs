using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.Patient;

namespace HealthSync.Core.Interfaces.Services;

public interface IPatientService
{
    Task<PaginatedResult<PatientResponseDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<PatientResponseDto?> GetByIdAsync(Guid id);
    Task<PatientResponseDto> CreateAsync(CreatePatientDto dto);
    Task<PatientResponseDto?> UpdateAsync(Guid id, UpdatePatientDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<PatientResponseDto>> SearchAsync(string query);
    Task<PaginatedResult<PatientResponseDto>> GetByDoctorIdAsync(Guid doctorId, int page, int pageSize, string? search);
}
