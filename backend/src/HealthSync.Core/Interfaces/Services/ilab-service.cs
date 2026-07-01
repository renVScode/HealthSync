using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.Lab;
using HealthSync.Core.Enums;

namespace HealthSync.Core.Interfaces.Services;

public interface ILabService
{
    // Lab Test Catalog
    Task<List<LabTestResponseDto>> GetAllLabTestsAsync();
    Task<PaginatedResult<LabTestResponseDto>> GetLabTestsAsync(int page, int pageSize, string? search);
    Task<LabTestResponseDto?> GetLabTestByIdAsync(Guid id);
    Task<LabTestResponseDto> CreateLabTestAsync(CreateLabTestDto dto);
    Task<LabTestResponseDto?> UpdateLabTestAsync(Guid id, UpdateLabTestDto dto);
    Task<bool> DeleteLabTestAsync(Guid id);

    // Lab Orders
    Task<List<LabOrderResponseDto>> GetAllLabOrdersAsync(LabOrderStatus? status = null, Guid? patientId = null, Guid? doctorId = null);
    Task<PaginatedResult<LabOrderResponseDto>> GetLabOrdersAsync(int page, int pageSize, LabOrderStatus? status, Guid? patientId, Guid? doctorId, string? search);
    Task<LabOrderResponseDto?> GetLabOrderByIdAsync(Guid id);
    Task<LabOrderResponseDto> CreateLabOrderAsync(Guid doctorId, CreateLabOrderDto dto);
    Task<LabOrderResponseDto?> UpdateLabOrderAsync(Guid id, UpdateLabOrderDto dto);
    Task<bool> DeleteLabOrderAsync(Guid id);
}
