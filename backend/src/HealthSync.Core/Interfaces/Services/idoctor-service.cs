using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.Doctor;

namespace HealthSync.Core.Interfaces.Services;

public interface IDoctorService
{
    Task<IEnumerable<DoctorResponseDto>> GetAllAsync();
    Task<PaginatedResult<DoctorResponseDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<DoctorResponseDto?> GetByIdAsync(Guid id);
    Task<DoctorResponseDto?> GetByUserIdAsync(string userId);
    Task<DoctorResponseDto> CreateAsync(CreateDoctorDto dto);
    Task<DoctorResponseDto?> UpdateAsync(Guid id, UpdateDoctorDto dto);
    Task<List<AvailabilityResponseDto>> GetAvailabilityAsync(Guid doctorId);
    Task UpdateAvailabilityAsync(Guid doctorId, List<UpsertAvailabilityDto> dtos);
    Task<List<TimeOffResponseDto>> GetTimeOffsAsync(Guid doctorId);
    Task<TimeOffResponseDto> RequestTimeOffAsync(Guid doctorId, CreateTimeOffDto dto);
    Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateTime date);
}
