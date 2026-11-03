using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WarehouseManagement.Products;

namespace WarehouseManagement.Barcodes;

public class ProductBarcode : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid BarcodeIdentityId { get; set; }

    public Guid ProductId { get; set; }

    public Guid ProductVariantId { get; set; }

    public BarcodePackagingLevel PackagingLevel { get; set; } = BarcodePackagingLevel.Each;

    public bool IsPrimary { get; set; }

    public LabelPrintFormat PrintFormat { get; set; } = LabelPrintFormat.Zpl_4x6;

    public int PrintedCount { get; set; }

    public DateTime? LastPrintedAt { get; set; }

    public BarcodeIdentity BarcodeIdentity { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;

    public ProductBarcode()
    {
    }

    public ProductBarcode(
        Guid id,
        Guid barcodeIdentityId,
        Guid productId,
        Guid productVariantId,
        BarcodePackagingLevel packagingLevel = BarcodePackagingLevel.Each,
        bool isPrimary = false,
        LabelPrintFormat printFormat = LabelPrintFormat.Zpl_4x6,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        BarcodeIdentityId = barcodeIdentityId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        PackagingLevel = packagingLevel;
        IsPrimary = isPrimary;
        PrintFormat = printFormat;
    }

    public void RecordPrintEvent()
    {
        PrintedCount++;
        LastPrintedAt = DateTime.UtcNow;
    }
}

