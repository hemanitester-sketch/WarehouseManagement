using System;

namespace WarehouseManagement.Inventory;

public class InventorySummaryDto
{
    public Guid ProductId { get; set; }
    public string ProductSku { get; set; } = null!;
    public string ProductName { get; set; } = null!;

    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = null!;

    public decimal TotalOnHand { get; set; }
    public decimal TotalAllocated { get; set; }
    public decimal TotalAvailable { get; set; }
    public decimal TotalQuarantined { get; set; }
    public decimal TotalDamaged { get; set; }

    public int LocationCount { get; set; }
}
