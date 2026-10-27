using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Warehouses;

public class GetWarehouseZoneListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public Guid? WarehouseId { get; set; }

    public WarehouseZoneStorageType? StorageType { get; set; }

    public bool? IsActive { get; set; }
}
