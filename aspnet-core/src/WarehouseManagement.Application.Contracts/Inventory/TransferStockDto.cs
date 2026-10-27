using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Inventory;

public class TransferStockDto
{
    [Required]
    public Guid SourceInventoryItemId { get; set; }

    [Required]
    public Guid DestinationWarehouseId { get; set; }

    [Required]
    public Guid DestinationLocationId { get; set; }

    [Required]
    public decimal Quantity { get; set; }

    [StringLength(InventoryConsts.MaxReferenceNumberLength)]
    public string? ReferenceNumber { get; set; }

    [StringLength(InventoryConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}
