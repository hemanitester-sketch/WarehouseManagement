using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Warehouses;

public class Warehouse : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public WarehouseType Type { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? ContactPerson { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public double TotalAreaSqFt { get; set; }

    public bool IsActive { get; set; }

    public Warehouse()
    {
    }

    public Warehouse(
        Guid id,
        string code,
        string name,
        WarehouseType type,
        Guid? tenantId = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null,
        string? contactPerson = null,
        string? contactPhone = null,
        string? contactEmail = null,
        double totalAreaSqFt = 0,
        bool isActive = true)
        : base(id)
    {
        TenantId = tenantId;
        SetCode(code);
        SetName(name);
        Type = type;
        Address = address;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        ContactPerson = contactPerson;
        ContactPhone = contactPhone;
        ContactEmail = contactEmail;
        TotalAreaSqFt = totalAreaSqFt;
        IsActive = isActive;
    }

    public void SetCode(string code)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code), WarehouseConsts.MaxCodeLength);
        Code = code.Trim().ToUpperInvariant();
    }

    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), WarehouseConsts.MaxNameLength);
        Name = name.Trim();
    }
}
