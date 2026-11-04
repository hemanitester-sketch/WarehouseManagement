using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Inventory;

public class InventoryItemDto : FullAuditedEntityDto<Guid>
{
    public Guid WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }

    public Guid ZoneId { get; set; }
    public string? ZoneCode { get; set; }

    public Guid LocationId { get; set; }
    public string? LocationBarcode { get; set; }
    public string? LocationCoordinates { get; set; }

    public Guid ProductId { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }

    public Guid? ProductVariantId { get; set; }
    public string? VariantSku { get; set; }
    public string? VariantName { get; set; }

    public decimal QuantityOnHand { get; set; }
    public decimal QuantityAllocated { get; set; }
    public decimal QuantityAvailable { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public decimal? PurchaseCost { get; set; }

    public string? LotBatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public string? Lpn { get; set; }

    public DateTime? ExpiryDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public bool IsExpired { get; set; }
    public bool IsNearExpiry { get; set; }
    public int? DaysUntilExpiry { get; set; }
    public InventoryStatus Status { get; set; }
    public DateTime ReceivedDate { get; set; }
}
