using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Products;

public class GetCategoryListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public bool? IsRootOnly { get; set; }

    public bool? IsActive { get; set; }
}
