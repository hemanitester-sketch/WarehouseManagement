using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Products;

public class ProductVariantAttributeValue : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid ProductVariantId { get; set; }

    public Guid ProductAttributeId { get; set; }

    public Guid? ProductAttributeValueId { get; set; }

    public string? CustomValue { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    public ProductAttribute ProductAttribute { get; set; } = null!;

    public ProductAttributeValue? ProductAttributeValue { get; set; }

    public ProductVariantAttributeValue()
    {
    }

    public ProductVariantAttributeValue(
        Guid id,
        Guid productVariantId,
        Guid productAttributeId,
        Guid? productAttributeValueId = null,
        string? customValue = null,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        ProductVariantId = productVariantId;
        ProductAttributeId = productAttributeId;
        ProductAttributeValueId = productAttributeValueId;
        CustomValue = customValue?.Trim();
    }
}

