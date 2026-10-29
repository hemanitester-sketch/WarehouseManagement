using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Suppliers;

public class CreateUpdateSupplierDto
{
    [Required]
    [StringLength(SupplierConsts.MaxCodeLength)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(SupplierConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [StringLength(SupplierConsts.MaxContactPersonLength)]
    public string? ContactPerson { get; set; }

    [EmailAddress]
    [StringLength(SupplierConsts.MaxEmailLength)]
    public string? Email { get; set; }

    [StringLength(SupplierConsts.MaxPhoneLength)]
    public string? PhoneNumber { get; set; }

    [StringLength(SupplierConsts.MaxTaxNumberLength)]
    public string? TaxNumber { get; set; }

    [StringLength(SupplierConsts.MaxAddressLength)]
    public string? Address { get; set; }

    [StringLength(SupplierConsts.MaxCityLength)]
    public string? City { get; set; }

    [StringLength(SupplierConsts.MaxStateLength)]
    public string? State { get; set; }

    [StringLength(SupplierConsts.MaxPostalCodeLength)]
    public string? PostalCode { get; set; }

    [StringLength(SupplierConsts.MaxPaymentTermsLength)]
    public string? PaymentTerms { get; set; } = "Net 30";

    public bool IsActive { get; set; } = true;

    [Range(1, 5)]
    public int? Rating { get; set; } = 5;

    [StringLength(SupplierConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}

