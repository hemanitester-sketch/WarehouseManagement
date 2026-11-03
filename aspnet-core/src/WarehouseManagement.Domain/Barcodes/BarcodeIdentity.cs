using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WarehouseManagement.Inventory;
using WarehouseManagement.Products;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Barcodes;

public class BarcodeIdentity : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string NormalizedBarcode { get; set; } = null!;

    public BarcodeTargetType BarcodeTargetType { get; set; }

    public BarcodeKind BarcodeKind { get; set; }

    public BarcodeSymbology Symbology { get; set; }

    public bool IsActive { get; set; } = true;

    // Exclusive Arc Target Foreign Keys
    public Guid? ProductVariantId { get; set; }

    public Guid? LicensePlateId { get; set; }

    public Guid? WarehouseLocationId { get; set; }

    public Guid? InventoryLotId { get; set; }

    public Guid? InventorySerialId { get; set; }

    // Navigation References
    public ProductVariant? ProductVariant { get; set; }

    public LicensePlate? LicensePlate { get; set; }

    public WarehouseLocation? WarehouseLocation { get; set; }

    public InventoryLot? InventoryLot { get; set; }

    public InventorySerial? InventorySerial { get; set; }

    public BarcodeIdentity()
    {
    }

    public BarcodeIdentity(
        Guid id,
        string rawBarcode,
        BarcodeTargetType targetType,
        BarcodeKind barcodeKind,
        BarcodeSymbology symbology,
        Guid? productVariantId = null,
        Guid? licensePlateId = null,
        Guid? warehouseLocationId = null,
        Guid? inventoryLotId = null,
        Guid? inventorySerialId = null,
        bool isActive = true,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        SetNormalizedBarcode(rawBarcode);
        BarcodeTargetType = targetType;
        BarcodeKind = barcodeKind;
        Symbology = symbology;
        ProductVariantId = productVariantId;
        LicensePlateId = licensePlateId;
        WarehouseLocationId = warehouseLocationId;
        InventoryLotId = inventoryLotId;
        InventorySerialId = inventorySerialId;
        IsActive = isActive;
        ValidateExclusiveArc();
    }

    public void SetNormalizedBarcode(string rawBarcode)
    {
        Check.NotNullOrWhiteSpace(rawBarcode, nameof(rawBarcode), BarcodeConsts.MaxBarcodeLength);
        NormalizedBarcode = rawBarcode.Trim().ToUpperInvariant();
    }

    public void ValidateExclusiveArc()
    {
        int nonNullCount = (ProductVariantId.HasValue ? 1 : 0) +
                           (LicensePlateId.HasValue ? 1 : 0) +
                           (WarehouseLocationId.HasValue ? 1 : 0) +
                           (InventoryLotId.HasValue ? 1 : 0) +
                           (InventorySerialId.HasValue ? 1 : 0);

        if (nonNullCount != 1)
        {
            throw new UserFriendlyException($"A barcode identity must reference exactly one target entity. Current non-null target count: {nonNullCount}");
        }

        bool match = (BarcodeTargetType == BarcodeTargetType.ProductVariant && ProductVariantId.HasValue) ||
                     (BarcodeTargetType == BarcodeTargetType.LicensePlate && LicensePlateId.HasValue) ||
                     (BarcodeTargetType == BarcodeTargetType.WarehouseLocation && WarehouseLocationId.HasValue) ||
                     (BarcodeTargetType == BarcodeTargetType.InventoryLot && InventoryLotId.HasValue) ||
                     (BarcodeTargetType == BarcodeTargetType.InventorySerial && InventorySerialId.HasValue);

        if (!match)
        {
            throw new UserFriendlyException($"BarcodeTargetType '{BarcodeTargetType}' does not match the populated target entity reference.");
        }
    }
}

