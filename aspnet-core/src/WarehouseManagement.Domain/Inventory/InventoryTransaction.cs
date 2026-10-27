using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Inventory;

public class InventoryTransaction : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? FromLocationId { get; set; }

    public Guid? ToLocationId { get; set; }

    public decimal Quantity { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? Lpn { get; set; }

    public InventoryTransactionType TransactionType { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public DateTime TransactionTime { get; set; }

    public InventoryTransaction()
    {
    }

    public InventoryTransaction(
        Guid id,
        Guid warehouseId,
        Guid productId,
        decimal quantity,
        InventoryTransactionType transactionType,
        Guid? tenantId = null,
        Guid? fromLocationId = null,
        Guid? toLocationId = null,
        string? lotBatchNumber = null,
        string? serialNumber = null,
        string? lpn = null,
        string? referenceNumber = null,
        string? notes = null,
        DateTime? transactionTime = null)
        : base(id)
    {
        WarehouseId = warehouseId;
        ProductId = productId;
        Quantity = quantity;
        TransactionType = transactionType;
        TenantId = tenantId;
        FromLocationId = fromLocationId;
        ToLocationId = toLocationId;
        LotBatchNumber = lotBatchNumber?.Trim();
        SerialNumber = serialNumber?.Trim();
        Lpn = lpn?.Trim();
        ReferenceNumber = referenceNumber?.Trim();
        Notes = notes?.Trim();
        TransactionTime = transactionTime ?? DateTime.UtcNow;
    }
}
