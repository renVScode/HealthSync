using HealthSync.Core.DTOs.Medicine;

namespace HealthSync.Core.Interfaces.Services;

public interface IMedicineService
{
    Task<IEnumerable<MedicineResponseDto>> GetAllAsync(string? search, string? category);
    Task<MedicineResponseDto?> GetByIdAsync(Guid id);
    Task<MedicineResponseDto> CreateAsync(CreateMedicineDto dto);
    Task<MedicineResponseDto?> UpdateAsync(Guid id, UpdateMedicineDto dto);
    Task DeactivateAsync(Guid id);
}
