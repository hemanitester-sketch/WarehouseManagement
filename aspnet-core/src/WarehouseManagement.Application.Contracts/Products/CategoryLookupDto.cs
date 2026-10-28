using System;

namespace WarehouseManagement.Products;

public class CategoryLookupDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public Guid? ParentCategoryId { get; set; }

    public string? ParentCategoryName { get; set; }

    public string DisplayName { get; set; } = null!;
}
