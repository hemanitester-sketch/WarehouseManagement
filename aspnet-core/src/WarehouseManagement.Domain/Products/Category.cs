using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Products;

public class Category : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// If null, this is a top-level parent category.
    /// If populated, this category is a subcategory of the specified parent.
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    public bool IsActive { get; set; }

    public Category()
    {
    }

    public Category(
        Guid id,
        string code,
        string name,
        Guid? parentCategoryId = null,
        string? description = null,
        bool isActive = true,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        SetCode(code);
        SetName(name);
        ParentCategoryId = parentCategoryId;
        Description = description;
        IsActive = isActive;
    }

    public Category SetCode(string code)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code), CategoryConsts.MaxCodeLength);
        Code = code.Trim().ToUpperInvariant();
        return this;
    }

    public Category SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), CategoryConsts.MaxNameLength);
        Name = name.Trim();
        return this;
    }
}
