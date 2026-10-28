using System;
using Volo.Abp.Application.Dtos;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Products;

public class ProductDto : FullAuditedEntityDto<Guid>
{
    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Barcode { get; set; }

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public ProductCategory Category { get; set; }

    public Guid? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string? CategoryCode { get; set; }

    public UnitOfMeasure Uom { get; set; }

    public bool IsLotTracked { get; set; }

    public bool IsExpiryTracked { get; set; }

    public bool IsSerialTracked { get; set; }

    public double WeightKg { get; set; }

    public double VolumeCm3 { get; set; }

    public double? LengthCm { get; set; }

    public double? WidthCm { get; set; }

    public double? HeightCm { get; set; }

    public WarehouseZoneStorageType RequiredStorageType { get; set; }

    public double? TemperatureMinCelsius { get; set; }

    public double? TemperatureMaxCelsius { get; set; }

    public int ReorderLevel { get; set; }

    public int TargetStockLevel { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; }
}
