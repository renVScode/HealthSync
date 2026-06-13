using HealthSync.Core.Entities;
using HealthSync.Core.DTOs.Medicine;

namespace HealthSync.Core.Interfaces.Services;

public interface IInventoryService
{
    Task<IEnumerable<InventoryBatchResponseDto>> GetAllAsync(Guid? medicineId, bool? expiringSoon);
    Task<InventoryBatchResponseDto?> GetByIdAsync(Guid id);
    Task<InventoryBatchResponseDto> AddBatchAsync(CreateInventoryBatchDto dto, string userId);
    Task<bool> DispenseAsync(Guid batchId, DispenseMedicineDto dto, string userId);
    Task<IEnumerable<LowStockItemDto>> GetLowStockItemsAsync();
    Task<IEnumerable<InventoryTransaction>> GetTransactionsAsync(DateTime? from, DateTime? to);
}
