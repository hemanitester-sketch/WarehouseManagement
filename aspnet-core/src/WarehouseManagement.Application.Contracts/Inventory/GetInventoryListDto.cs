using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Inventory;

public class GetInventoryListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid? ZoneId { get; set; }

    public Guid? LocationId { get; set; }

    public Guid? ProductId { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? Lpn { get; set; }

    public InventoryStatus? Status { get; set; }

    public bool? ExpiredOnly { get; set; }
}
