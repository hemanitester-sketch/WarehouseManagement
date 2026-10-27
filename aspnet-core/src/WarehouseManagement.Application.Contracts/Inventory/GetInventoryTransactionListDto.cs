using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Inventory;

public class GetInventoryTransactionListDto : PagedAndSortedResultRequestDto
{
    public Guid? WarehouseId { get; set; }
    public Guid? ProductId { get; set; }
    public InventoryTransactionType? TransactionType { get; set; }
    public string? ReferenceNumber { get; set; }
}
