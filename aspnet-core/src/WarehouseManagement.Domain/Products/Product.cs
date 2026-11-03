using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Products;

public class Product : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public ProductType ProductType { get; set; } = ProductType.Simple;

    public string? Barcode { get; set; }

    public string? ManufacturerPartNumber { get; set; }

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public string? ImageUrl { get; set; }

    public ProductCategory Category { get; set; }

    public Guid? CategoryId { get; set; }

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

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    public Product()
    {
    }

    public Product(
        Guid id,
        string sku,
        string name,
        ProductCategory category,
        UnitOfMeasure uom,
        Guid? tenantId = null,
        string? barcode = null,
        string? description = null,
        string? brand = null,
        bool isLotTracked = false,
        bool isExpiryTracked = false,
        bool isSerialTracked = false,
        decimal weightKg = 0,
        decimal volumeCm3 = 0,
        decimal? lengthCm = null,
        decimal? widthCm = null,
        decimal? heightCm = null,
        WarehouseZoneStorageType requiredStorageType = WarehouseZoneStorageType.Ambient,
        double? temperatureMinCelsius = null,
        double? temperatureMaxCelsius = null,
        int reorderLevel = 10,
        int targetStockLevel = 100,
        decimal unitPrice = 0,
        bool isActive = true,
        Guid? categoryId = null,
        ProductType productType = ProductType.Simple,
        string? manufacturerPartNumber = null)
        : base(id)
    {
        TenantId = tenantId;
        SetSku(sku);
        SetName(name);
        Barcode = barcode?.Trim();
        Description = description;
        Brand = brand;
        Category = category;
        Uom = uom;
        IsLotTracked = isLotTracked;
        IsExpiryTracked = isExpiryTracked;
        IsSerialTracked = isSerialTracked;
        WeightKg = weightKg;
        VolumeCm3 = volumeCm3;
        LengthCm = lengthCm;
        WidthCm = widthCm;
        HeightCm = heightCm;
        RequiredStorageType = requiredStorageType;
        TemperatureMinCelsius = temperatureMinCelsius;
        TemperatureMaxCelsius = temperatureMaxCelsius;
        ReorderLevel = reorderLevel;
        TargetStockLevel = targetStockLevel;
        UnitPrice = unitPrice;
        IsActive = isActive;
        CategoryId = categoryId;
        ProductType = productType;
        ManufacturerPartNumber = manufacturerPartNumber?.Trim();
    }

    public void SetSku(string sku)
    {
        Check.NotNullOrWhiteSpace(sku, nameof(sku), ProductConsts.MaxSkuLength);
        Sku = sku.Trim().ToUpperInvariant();
    }

    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), ProductConsts.MaxNameLength);
        Name = name.Trim();
    }
}
