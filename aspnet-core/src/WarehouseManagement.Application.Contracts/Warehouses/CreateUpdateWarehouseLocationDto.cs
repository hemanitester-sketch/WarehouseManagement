using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Warehouses;

public class CreateUpdateWarehouseLocationDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public Guid ZoneId { get; set; }

    [Required]
    [StringLength(WarehouseLocationConsts.MaxBarcodeLength)]
    public string Barcode { get; set; } = null!;

    [Required]
    [StringLength(WarehouseLocationConsts.MaxAisleLength)]
    public string Aisle { get; set; } = null!;

    [Required]
    [StringLength(WarehouseLocationConsts.MaxRackLength)]
    public string Rack { get; set; } = null!;

    [Required]
    [StringLength(WarehouseLocationConsts.MaxShelfLength)]
    public string Shelf { get; set; } = null!;

    [Required]
    [StringLength(WarehouseLocationConsts.MaxBinLength)]
    public string Bin { get; set; } = null!;

    public WarehouseLocationType LocationType { get; set; } = WarehouseLocationType.PalletRack;

    public WarehouseLocationStatus Status { get; set; } = WarehouseLocationStatus.Available;

    public double MaxWeightKg { get; set; } = 1000;

    public double MaxVolumeCm3 { get; set; } = 1000000;

    public double? LengthCm { get; set; }

    public double? WidthCm { get; set; }

    public double? HeightCm { get; set; }

    public bool IsPickFace { get; set; } = false;

    public bool IsActive { get; set; } = true;
}
