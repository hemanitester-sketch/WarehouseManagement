using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Warehouses;

public class GetWarehouseListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public WarehouseType? Type { get; set; }

    public bool? IsActive { get; set; }
}
