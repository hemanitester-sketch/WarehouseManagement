using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Inventory;

public class InventoryTransactionDto : EntityDto<Guid>
{
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
}
