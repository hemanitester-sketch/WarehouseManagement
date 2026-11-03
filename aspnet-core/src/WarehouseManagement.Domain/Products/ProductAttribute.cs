using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Products;

public class ProductAttribute : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public AttributeDataType DataType { get; set; }

    public bool IsVariantDefining { get; set; }

    public bool IsMandatory { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();

    public ProductAttribute()
    {
    }

    public ProductAttribute(
        Guid id,
        string code,
        string name,
        AttributeDataType dataType,
        bool isVariantDefining = false,
        bool isMandatory = false,
        bool isActive = true,
        int displayOrder = 0,
        string? description = null,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        SetCode(code);
        SetName(name);
        DataType = dataType;
        IsVariantDefining = isVariantDefining;
        IsMandatory = isMandatory;
        IsActive = isActive;
        DisplayOrder = displayOrder;
        Description = description?.Trim();
    }

    public void SetCode(string code)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code), ProductAttributeConsts.MaxCodeLength);
        Code = code.Trim().ToUpperInvariant();
    }

    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), ProductAttributeConsts.MaxNameLength);
        Name = name.Trim();
    }
}

