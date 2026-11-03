using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Products;

public class ProductAttributeValue : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid ProductAttributeId { get; set; }

    public string Value { get; set; } = null!;

    public string? DisplayCode { get; set; }

    public string? ColorHexCode { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ProductAttribute ProductAttribute { get; set; } = null!;

    public ProductAttributeValue()
    {
    }

    public ProductAttributeValue(
        Guid id,
        Guid productAttributeId,
        string value,
        string? displayCode = null,
        string? colorHexCode = null,
        int displayOrder = 0,
        bool isActive = true,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        ProductAttributeId = productAttributeId;
        SetValue(value);
        DisplayCode = displayCode?.Trim().ToUpperInvariant();
        ColorHexCode = colorHexCode?.Trim();
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public void SetValue(string value)
    {
        Check.NotNullOrWhiteSpace(value, nameof(value), ProductAttributeConsts.MaxValueLength);
        Value = value.Trim();
    }
}

