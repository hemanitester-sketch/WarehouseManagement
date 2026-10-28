using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Products;

public class CreateUpdateCategoryDto
{
    [Required]
    [StringLength(CategoryConsts.MaxCodeLength)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(CategoryConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [StringLength(CategoryConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}
