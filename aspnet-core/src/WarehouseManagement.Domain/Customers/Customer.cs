using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Customers;

public class Customer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public CustomerType CustomerType { get; set; } = CustomerType.CorporateB2B;

    public string? Industry { get; set; }

    public string? ContactPerson { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AlternativePhone { get; set; }

    public string? TaxNumber { get; set; } // GSTIN

    public string? PanNumber { get; set; }

    public string? BillingAddress { get; set; }

    public string? ShippingAddress { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; } = "India";

    public string? PaymentTerms { get; set; } = "Net 30";

    public decimal CreditLimit { get; set; } = 500000m;

    public decimal CurrentOutstanding { get; set; } = 0m;

    public string? PriceTier { get; set; } = "Tier 2 (Standard B2B)";

    public string? DeliveryDockHours { get; set; }

    public string? SpecialInstructions { get; set; }

    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    public bool IsActive { get; set; } = true;

    public int? Rating { get; set; } = 5;

    public int TotalOrdersPlaced { get; set; } = 0;

    public decimal TotalRevenue { get; set; } = 0m;

    public string? Notes { get; set; }

    public Customer()
    {
    }

    public Customer(
        Guid id,
        string code,
        string name,
        CustomerType customerType = CustomerType.CorporateB2B,
        string? industry = null,
        string? contactPerson = null,
        string? email = null,
        string? phoneNumber = null,
        string? alternativePhone = null,
        string? taxNumber = null,
        string? panNumber = null,
        string? billingAddress = null,
        string? shippingAddress = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = "India",
        string? paymentTerms = "Net 30",
        decimal creditLimit = 500000m,
        decimal currentOutstanding = 0m,
        string? priceTier = "Tier 2 (Standard B2B)",
        string? deliveryDockHours = null,
        string? specialInstructions = null,
        CustomerStatus status = CustomerStatus.Active,
        bool isActive = true,
        int? rating = 5,
        int totalOrdersPlaced = 0,
        decimal totalRevenue = 0m,
        string? notes = null,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        SetCode(code);
        SetName(name);
        CustomerType = customerType;
        Industry = industry?.Trim();
        ContactPerson = contactPerson?.Trim();
        Email = email?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        AlternativePhone = alternativePhone?.Trim();
        TaxNumber = taxNumber?.Trim();
        PanNumber = panNumber?.Trim();
        BillingAddress = billingAddress?.Trim();
        ShippingAddress = shippingAddress?.Trim();
        City = city?.Trim();
        State = state?.Trim();
        PostalCode = postalCode?.Trim();
        Country = country?.Trim() ?? "India";
        PaymentTerms = paymentTerms?.Trim() ?? "Net 30";
        CreditLimit = creditLimit >= 0 ? creditLimit : 0m;
        CurrentOutstanding = currentOutstanding >= 0 ? currentOutstanding : 0m;
        PriceTier = priceTier?.Trim() ?? "Tier 2 (Standard B2B)";
        DeliveryDockHours = deliveryDockHours?.Trim();
        SpecialInstructions = specialInstructions?.Trim();
        Status = status;
        IsActive = isActive;
        Rating = rating;
        TotalOrdersPlaced = totalOrdersPlaced;
        TotalRevenue = totalRevenue;
        Notes = notes?.Trim();
    }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), CustomerConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), CustomerConsts.MaxNameLength).Trim();
    }
}

