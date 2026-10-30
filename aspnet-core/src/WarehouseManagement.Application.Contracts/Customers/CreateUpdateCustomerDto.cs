using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Customers;

public class CreateUpdateCustomerDto
{
    [Required]
    [StringLength(CustomerConsts.MaxCodeLength)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(CustomerConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    public CustomerType CustomerType { get; set; } = CustomerType.CorporateB2B;

    [StringLength(CustomerConsts.MaxIndustryLength)]
    public string? Industry { get; set; }

    [StringLength(CustomerConsts.MaxContactPersonLength)]
    public string? ContactPerson { get; set; }

    [EmailAddress]
    [StringLength(CustomerConsts.MaxEmailLength)]
    public string? Email { get; set; }

    [StringLength(CustomerConsts.MaxPhoneLength)]
    public string? PhoneNumber { get; set; }

    [StringLength(CustomerConsts.MaxPhoneLength)]
    public string? AlternativePhone { get; set; }

    [StringLength(CustomerConsts.MaxTaxNumberLength)]
    public string? TaxNumber { get; set; }

    [StringLength(CustomerConsts.MaxPanNumberLength)]
    public string? PanNumber { get; set; }

    [StringLength(CustomerConsts.MaxAddressLength)]
    public string? BillingAddress { get; set; }

    [StringLength(CustomerConsts.MaxAddressLength)]
    public string? ShippingAddress { get; set; }

    [StringLength(CustomerConsts.MaxCityLength)]
    public string? City { get; set; }

    [StringLength(CustomerConsts.MaxStateLength)]
    public string? State { get; set; }

    [StringLength(CustomerConsts.MaxPostalCodeLength)]
    public string? PostalCode { get; set; }

    [StringLength(CustomerConsts.MaxCountryLength)]
    public string? Country { get; set; } = "India";

    [StringLength(CustomerConsts.MaxPaymentTermsLength)]
    public string? PaymentTerms { get; set; } = "Net 30";

    public decimal CreditLimit { get; set; } = 500000m;

    public decimal CurrentOutstanding { get; set; } = 0m;

    [StringLength(CustomerConsts.MaxPriceTierLength)]
    public string? PriceTier { get; set; } = "Tier 2 (Standard B2B)";

    [StringLength(CustomerConsts.MaxDeliveryDockHoursLength)]
    public string? DeliveryDockHours { get; set; }

    [StringLength(CustomerConsts.MaxSpecialInstructionsLength)]
    public string? SpecialInstructions { get; set; }

    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    public bool IsActive { get; set; } = true;

    [Range(1, 5)]
    public int? Rating { get; set; } = 5;

    [StringLength(CustomerConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}

