using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WarehouseManagement.Products;
using WarehouseManagement.Suppliers;

namespace WarehouseManagement.Barcodes;

public class SupplierBarcodeMapping : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid SupplierId { get; set; }

    public Guid BarcodeIdentityId { get; set; }

    public Guid ProductVariantId { get; set; }

    public string? SupplierPartNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public Supplier Supplier { get; set; } = null!;

    public BarcodeIdentity BarcodeIdentity { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;

    public SupplierBarcodeMapping()
    {
    }

    public SupplierBarcodeMapping(
        Guid id,
        Guid supplierId,
        Guid barcodeIdentityId,
        Guid productVariantId,
        string? supplierPartNumber = null,
        bool isActive = true,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        SupplierId = supplierId;
        BarcodeIdentityId = barcodeIdentityId;
        ProductVariantId = productVariantId;
        SupplierPartNumber = supplierPartNumber?.Trim();
        IsActive = isActive;
    }
}

