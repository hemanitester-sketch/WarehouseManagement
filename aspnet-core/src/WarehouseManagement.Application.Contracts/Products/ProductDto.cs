using System;
using Volo.Abp.Application.Dtos;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Products;

public class ProductDto : FullAuditedEntityDto<Guid>
{
    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Barcode { get; set; }

    public ProductType ProductType { get; set; } = ProductType.Simple;

    public string? ManufacturerPartNumber { get; set; }

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public string? ImageUrl { get; set; }

    public ProductCategory Category { get; set; }

    public Guid? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string? CategoryCode { get; set; }

    public UnitOfMeasure Uom { get; set; }

    public bool IsLotTracked { get; set; }

    public bool IsExpiryTracked { get; set; }

    public bool IsSerialTracked { get; set; }

    public decimal WeightKg { get; set; }

    public decimal VolumeCm3 { get; set; }

    public decimal? LengthCm { get; set; }

    public decimal? WidthCm { get; set; }

    public decimal? HeightCm { get; set; }

    public WarehouseZoneStorageType RequiredStorageType { get; set; }

    public double? TemperatureMinCelsius { get; set; }

    public double? TemperatureMaxCelsius { get; set; }

    public int ReorderLevel { get; set; }

    public int TargetStockLevel { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; }

    public int VariantCount { get; set; }
}
