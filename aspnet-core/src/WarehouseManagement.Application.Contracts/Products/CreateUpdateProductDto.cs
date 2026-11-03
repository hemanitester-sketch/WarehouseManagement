using System;
using System.ComponentModel.DataAnnotations;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Products;

public class CreateUpdateProductDto
{
    [Required]
    [StringLength(ProductConsts.MaxSkuLength)]
    public string Sku { get; set; } = null!;

    [Required]
    [StringLength(ProductConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [StringLength(ProductConsts.MaxBarcodeLength)]
    public string? Barcode { get; set; }

    public ProductType ProductType { get; set; } = ProductType.Simple;

    [StringLength(ProductConsts.MaxManufacturerPartNumberLength)]
    public string? ManufacturerPartNumber { get; set; }

    [StringLength(ProductConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [StringLength(ProductConsts.MaxBrandLength)]
    public string? Brand { get; set; }

    [StringLength(512)]
    public string? ImageUrl { get; set; }

    public ProductCategory Category { get; set; } = ProductCategory.General;

    public Guid? CategoryId { get; set; }

    public UnitOfMeasure Uom { get; set; } = UnitOfMeasure.Pieces;

    public bool IsLotTracked { get; set; } = false;

    public bool IsExpiryTracked { get; set; } = false;

    public bool IsSerialTracked { get; set; } = false;

    public decimal WeightKg { get; set; } = 0;

    public decimal VolumeCm3 { get; set; } = 0;

    public decimal? LengthCm { get; set; }

    public decimal? WidthCm { get; set; }

    public decimal? HeightCm { get; set; }

    public WarehouseZoneStorageType RequiredStorageType { get; set; } = WarehouseZoneStorageType.Ambient;

    public double? TemperatureMinCelsius { get; set; }

    public double? TemperatureMaxCelsius { get; set; }

    public int ReorderLevel { get; set; } = 10;

    public int TargetStockLevel { get; set; } = 100;

    public decimal UnitPrice { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}
