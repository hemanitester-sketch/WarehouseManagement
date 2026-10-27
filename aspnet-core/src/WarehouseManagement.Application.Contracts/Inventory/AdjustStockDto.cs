using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Inventory;

public class AdjustStockDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public Guid LocationId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public decimal QuantityDelta { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? Lpn { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime? ManufactureDate { get; set; }

    public InventoryStatus Status { get; set; } = InventoryStatus.Available;

    [Required]
    [StringLength(InventoryConsts.MaxReferenceNumberLength)]
    public string Reason { get; set; } = null!;

    [StringLength(InventoryConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}
