using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Customers;

public class CustomerDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public CustomerType CustomerType { get; set; }

    public string CustomerTypeName => CustomerType.ToString();

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

    public string? Country { get; set; }

    public string? PaymentTerms { get; set; }

    public decimal CreditLimit { get; set; }

    public decimal CurrentOutstanding { get; set; }

    public decimal CreditUtilizationPercent => CreditLimit > 0 ? Math.Min(100m, Math.Round((CurrentOutstanding / CreditLimit) * 100m, 1)) : 0m;

    public string? PriceTier { get; set; }

    public string? DeliveryDockHours { get; set; }

    public string? SpecialInstructions { get; set; }

    public CustomerStatus Status { get; set; }

    public string StatusName => Status.ToString();

    public bool IsActive { get; set; }

    public int? Rating { get; set; }

    public int TotalOrdersPlaced { get; set; }

    public decimal TotalRevenue { get; set; }

    public string? Notes { get; set; }
}

