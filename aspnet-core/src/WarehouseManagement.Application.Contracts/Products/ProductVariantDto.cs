using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using WarehouseManagement.Barcodes;

namespace WarehouseManagement.Products;

public class ProductVariantDto : FullAuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }

    public string ProductSku { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public string? VariantName { get; set; }

    public string CombinationHash { get; set; } = null!;

    public decimal? PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal WeightKg { get; set; }

    public decimal VolumeCm3 { get; set; }

    public decimal? LengthCm { get; set; }

    public decimal? WidthCm { get; set; }

    public decimal? HeightCm { get; set; }

    public bool IsActive { get; set; }

    public bool IsDefault { get; set; }

    public string? ImageUrl { get; set; }

    public string? PrimaryBarcode { get; set; }

    public List<ProductVariantAttributeValueDto> Attributes { get; set; } = new();

    public List<ProductBarcodeDto> Barcodes { get; set; } = new();
}

public class ProductVariantAttributeValueDto
{
    public Guid AttributeId { get; set; }

    public string AttributeName { get; set; } = null!;

    public string AttributeCode { get; set; } = null!;

    public Guid? ValueId { get; set; }

    public string ValueText { get; set; } = null!;
}

public class ProductBarcodeDto : EntityDto<Guid>
{
    public string BarcodeValue { get; set; } = null!;

    public BarcodeSymbology Symbology { get; set; }

    public BarcodeKind BarcodeKind { get; set; }

    public BarcodePackagingLevel PackagingLevel { get; set; }

    public int UnitsPerPackage { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }
}

public class CreateProductVariantDto
{
    [Required]
    public Guid ProductId { get; set; }

    [StringLength(ProductConsts.MaxSkuLength)]
    public string? Sku { get; set; }

    [StringLength(ProductConsts.MaxNameLength)]
    public string? VariantName { get; set; }

    public decimal? PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; } = 0;

    public decimal WeightKg { get; set; } = 0;

    public decimal VolumeCm3 { get; set; } = 0;

    public decimal? LengthCm { get; set; }

    public decimal? WidthCm { get; set; }

    public decimal? HeightCm { get; set; }

    public bool IsDefault { get; set; } = false;

    [StringLength(512)]
    public string? ImageUrl { get; set; }

    public string? PrimaryBarcode { get; set; }

    public List<VariantAttributeSelectionDto> Attributes { get; set; } = new();
}

public class VariantAttributeSelectionDto
{
    [Required]
    public Guid AttributeId { get; set; }

    public Guid? ValueId { get; set; }

    [Required]
    public string ValueText { get; set; } = null!;
}

public class UpdateProductVariantDto
{
    [Required]
    [StringLength(ProductConsts.MaxSkuLength)]
    public string Sku { get; set; } = null!;

    [StringLength(ProductConsts.MaxNameLength)]
    public string? VariantName { get; set; }

    public decimal? PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal WeightKg { get; set; }

    public decimal VolumeCm3 { get; set; }

    public decimal? LengthCm { get; set; }

    public decimal? WidthCm { get; set; }

    public decimal? HeightCm { get; set; }

    public bool IsActive { get; set; }

    public bool IsDefault { get; set; }
}

public class GenerateVariantMatrixInput
{
    [Required]
    public Guid ProductId { get; set; }

    public List<AttributeMatrixOptionInput> Options { get; set; } = new();
}

public class AttributeMatrixOptionInput
{
    public Guid AttributeId { get; set; }

    public string AttributeName { get; set; } = null!;

    public string AttributeCode { get; set; } = null!;

    public List<string> SelectedValues { get; set; } = new();
}

public class GeneratedVariantPreviewDto
{
    public string GeneratedSku { get; set; } = null!;

    public string GeneratedVariantName { get; set; } = null!;

    public string CombinationHash { get; set; } = null!;

    public List<VariantAttributeSelectionDto> Attributes { get; set; } = new();
}

