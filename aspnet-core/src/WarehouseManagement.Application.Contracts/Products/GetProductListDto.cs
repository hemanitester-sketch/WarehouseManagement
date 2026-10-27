using Volo.Abp.Application.Dtos;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Products;

public class GetProductListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public ProductCategory? Category { get; set; }

    public UnitOfMeasure? Uom { get; set; }

    public WarehouseZoneStorageType? RequiredStorageType { get; set; }

    public bool? IsLotTracked { get; set; }

    public bool? IsExpiryTracked { get; set; }

    public bool? IsSerialTracked { get; set; }

    public bool? IsActive { get; set; }
}
