using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Products;

public class CategoryAttributeTemplate : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CategoryId { get; set; }

    public Guid ProductAttributeId { get; set; }

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public Category Category { get; set; } = null!;

    public ProductAttribute ProductAttribute { get; set; } = null!;

    public CategoryAttributeTemplate()
    {
    }

    public CategoryAttributeTemplate(
        Guid id,
        Guid categoryId,
        Guid productAttributeId,
        bool isRequired = false,
        int displayOrder = 0,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        CategoryId = categoryId;
        ProductAttributeId = productAttributeId;
        IsRequired = isRequired;
        DisplayOrder = displayOrder;
    }
}

