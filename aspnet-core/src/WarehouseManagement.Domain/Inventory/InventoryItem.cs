using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Inventory;

public class InventoryItem : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid ZoneId { get; set; }

    public Guid LocationId { get; set; }

    public Guid ProductId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public decimal QuantityAllocated { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? Lpn { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime? ManufactureDate { get; set; }

    public InventoryStatus Status { get; set; }

    public DateTime ReceivedDate { get; set; }

    public decimal QuantityAvailable => Math.Max(0, QuantityOnHand - QuantityAllocated);

    public InventoryItem()
    {
    }

    public InventoryItem(
        Guid id,
        Guid warehouseId,
        Guid zoneId,
        Guid locationId,
        Guid productId,
        decimal quantityOnHand,
        Guid? tenantId = null,
        string? lotBatchNumber = null,
        string? serialNumber = null,
        string? lpn = null,
        DateTime? expiryDate = null,
        DateTime? manufactureDate = null,
        InventoryStatus status = InventoryStatus.Available,
        DateTime? receivedDate = null,
        decimal quantityAllocated = 0)
        : base(id)
    {
        WarehouseId = warehouseId;
        ZoneId = zoneId;
        LocationId = locationId;
        ProductId = productId;
        TenantId = tenantId;
        QuantityOnHand = Math.Max(0, quantityOnHand);
        QuantityAllocated = Math.Max(0, quantityAllocated);
        LotBatchNumber = lotBatchNumber?.Trim();
        SerialNumber = serialNumber?.Trim();
        Lpn = lpn?.Trim();
        ExpiryDate = expiryDate;
        ManufactureDate = manufactureDate;
        Status = status;
        ReceivedDate = receivedDate ?? DateTime.UtcNow;
    }

    public void AdjustQuantity(decimal delta)
    {
        var newQty = QuantityOnHand + delta;
        if (newQty < 0)
        {
            throw new UserFriendlyException($"Cannot reduce stock below zero. Current: {QuantityOnHand}, delta: {delta}");
        }
        QuantityOnHand = newQty;
    }

    public void AllocateQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        if (QuantityAvailable < quantity)
        {
            throw new UserFriendlyException($"Insufficient available quantity to allocate. Available: {QuantityAvailable}, Requested: {quantity}");
        }

        QuantityAllocated += quantity;
    }

    public void ReleaseAllocatedQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        QuantityAllocated = Math.Max(0, QuantityAllocated - quantity);
    }

    public void MoveToLocation(Guid newWarehouseId, Guid newZoneId, Guid newLocationId)
    {
        WarehouseId = newWarehouseId;
        ZoneId = newZoneId;
        LocationId = newLocationId;
    }

    public void SetStatus(InventoryStatus status)
    {
        Status = status;
    }
}
