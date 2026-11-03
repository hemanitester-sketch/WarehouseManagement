using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Products;

public class ProductVariant : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid ProductId { get; set; }

    public string VariantSku { get; set; } = null!;

    public string VariantName { get; set; } = null!;

    public string CombinationHash { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public decimal PurchaseCost { get; set; }

    public decimal WeightKg { get; set; }

    public decimal? LengthCm { get; set; }

    public decimal? WidthCm { get; set; }

    public decimal? HeightCm { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public Product Product { get; set; } = null!;

    public ICollection<ProductVariantAttributeValue> AttributeValues { get; set; } = new List<ProductVariantAttributeValue>();

    public ICollection<WarehouseManagement.Barcodes.ProductBarcode> Barcodes { get; set; } = new List<WarehouseManagement.Barcodes.ProductBarcode>();

    public ProductVariant()
    {
    }

    public ProductVariant(
        Guid id,
        Guid productId,
        string variantSku,
        string variantName,
        string combinationHash,
        decimal unitPrice = 0,
        decimal purchaseCost = 0,
        decimal weightKg = 0,
        decimal? lengthCm = null,
        decimal? widthCm = null,
        decimal? heightCm = null,
        string? imageUrl = null,
        bool isDefault = false,
        bool isActive = true,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        SetVariantSku(variantSku);
        SetVariantName(variantName);
        SetCombinationHash(combinationHash);
        UnitPrice = unitPrice;
        PurchaseCost = purchaseCost;
        WeightKg = weightKg;
        LengthCm = lengthCm;
        WidthCm = widthCm;
        HeightCm = heightCm;
        ImageUrl = imageUrl?.Trim();
        IsDefault = isDefault;
        IsActive = isActive;
    }

    public void SetVariantSku(string variantSku)
    {
        Check.NotNullOrWhiteSpace(variantSku, nameof(variantSku), ProductVariantConsts.MaxSkuLength);
        VariantSku = variantSku.Trim().ToUpperInvariant();
    }

    public void SetVariantName(string variantName)
    {
        Check.NotNullOrWhiteSpace(variantName, nameof(variantName), ProductVariantConsts.MaxNameLength);
        VariantName = variantName.Trim();
    }

    public void SetCombinationHash(string combinationHash)
    {
        Check.NotNullOrWhiteSpace(combinationHash, nameof(combinationHash), ProductVariantConsts.MaxCombinationHashLength);
        CombinationHash = combinationHash.Trim();
    }
}

