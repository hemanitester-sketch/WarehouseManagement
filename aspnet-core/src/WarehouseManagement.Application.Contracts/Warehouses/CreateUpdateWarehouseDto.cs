using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Warehouses;

public class CreateUpdateWarehouseDto
{
    [Required(ErrorMessage = "Warehouse code is required.")]
    [StringLength(WarehouseConsts.MaxCodeLength)]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Warehouse name is required.")]
    [StringLength(WarehouseConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [Required]
    public WarehouseType Type { get; set; } = WarehouseType.DistributionCenter;

    [Required(ErrorMessage = "Street address is required.")]
    [StringLength(WarehouseConsts.MaxAddressLength)]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "City is required.")]
    [StringLength(WarehouseConsts.MaxCityLength)]
    public string City { get; set; } = null!;

    [Required(ErrorMessage = "State is required.")]
    [StringLength(WarehouseConsts.MaxStateLength)]
    public string State { get; set; } = null!;

    [Required(ErrorMessage = "Postal code is required.")]
    [StringLength(WarehouseConsts.MaxPostalCodeLength)]
    public string PostalCode { get; set; } = null!;

    [StringLength(WarehouseConsts.MaxCountryLength)]
    public string? Country { get; set; } = "India";

    [Required(ErrorMessage = "Facility manager is required.")]
    [StringLength(WarehouseConsts.MaxContactNameLength)]
    public string ContactPerson { get; set; } = null!;

    [Required(ErrorMessage = "Contact phone is required.")]
    [StringLength(WarehouseConsts.MaxPhoneLength)]
    public string ContactPhone { get; set; } = null!;

    [EmailAddress]
    [StringLength(WarehouseConsts.MaxEmailLength)]
    public string? ContactEmail { get; set; }

    [Range(100, 100_000_000, ErrorMessage = "Total floor area must be at least 100 sq.ft.")]
    public double TotalAreaSqFt { get; set; }

    public bool IsActive { get; set; } = true;
}
