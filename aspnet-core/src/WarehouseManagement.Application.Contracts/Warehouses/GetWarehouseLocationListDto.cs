using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Warehouses;

public class GetWarehouseLocationListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid? ZoneId { get; set; }

    public WarehouseLocationType? LocationType { get; set; }

    public WarehouseLocationStatus? Status { get; set; }

    public bool? IsPickFace { get; set; }

    public bool? IsActive { get; set; }
}
