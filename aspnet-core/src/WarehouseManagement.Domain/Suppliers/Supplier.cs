using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Suppliers;

public class Supplier : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? ContactPerson { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? TaxNumber { get; set; } // GSTIN / VAT / PAN

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? PaymentTerms { get; set; } // e.g. Net 30, Net 15, Immediate, Advance

    public bool IsActive { get; set; } = true;

    public int? Rating { get; set; } // 1-5 stars

    public string? Notes { get; set; }

    public Supplier()
    {
    }

    public Supplier(
        Guid id,
        string code,
        string name,
        string? contactPerson = null,
        string? email = null,
        string? phoneNumber = null,
        string? taxNumber = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? paymentTerms = "Net 30",
        bool isActive = true,
        int? rating = 5,
        string? notes = null,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        SetCode(code);
        SetName(name);
        ContactPerson = contactPerson?.Trim();
        Email = email?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        TaxNumber = taxNumber?.Trim();
        Address = address?.Trim();
        City = city?.Trim();
        State = state?.Trim();
        PostalCode = postalCode?.Trim();
        PaymentTerms = paymentTerms?.Trim() ?? "Net 30";
        IsActive = isActive;
        Rating = rating;
        Notes = notes?.Trim();
    }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), SupplierConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), SupplierConsts.MaxNameLength).Trim();
    }
}

