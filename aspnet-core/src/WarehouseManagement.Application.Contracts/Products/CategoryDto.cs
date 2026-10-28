using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Products;

public class CategoryDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public string? ParentCategoryName { get; set; }

    public string? ParentCategoryCode { get; set; }

    public int SubCategoriesCount { get; set; }

    public bool IsActive { get; set; }
}
