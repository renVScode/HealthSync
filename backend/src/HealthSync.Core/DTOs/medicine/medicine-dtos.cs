namespace HealthSync.Core.DTOs.Medicine;

public class CreateMedicineDto
{
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Category { get; set; }
    public string? Manufacturer { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public string? Description { get; set; }
}

public class UpdateMedicineDto
{
    public string? Name { get; set; }
    public string? GenericName { get; set; }
    public string? Category { get; set; }
    public string? Manufacturer { get; set; }
    public string? Unit { get; set; }
    public decimal? Price { get; set; }
    public int? ReorderLevel { get; set; }
    public string? Description { get; set; }
}

public class MedicineResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Category { get; set; }
    public string? Manufacturer { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int ReorderLevel { get; set; }
    public int CurrentStock { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CreateInventoryBatchDto
{
    public Guid MedicineId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Supplier { get; set; }
    public string? Location { get; set; }
}

public class InventoryBatchResponseDto
{
    public Guid Id { get; set; }
    public Guid MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Supplier { get; set; }
    public bool IsLowStock { get; set; }
}

public class DispenseMedicineDto
{
    public int Quantity { get; set; }
    public Guid? PrescriptionId { get; set; }
    public string? Notes { get; set; }
}

public class LowStockItemDto
{
    public Guid MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
}
