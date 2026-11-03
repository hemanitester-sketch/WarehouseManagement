using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WarehouseManagement.Products;

namespace WarehouseManagement.Inventory;

public class InventoryLot : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid ProductId { get; set; }

    public Guid ProductVariantId { get; set; }

    public string LotNumber { get; set; } = null!;

    public DateTime? ManufactureDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? SupplierLotNumber { get; set; }

    public LotStatus Status { get; set; } = LotStatus.Available;

    public string? CertificateUrl { get; set; }

    public string? Notes { get; set; }

    public Product Product { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;

    public InventoryLot()
    {
    }

    public InventoryLot(
        Guid id,
        Guid productId,
        Guid productVariantId,
        string lotNumber,
        DateTime? manufactureDate = null,
        DateTime? expiryDate = null,
        string? supplierLotNumber = null,
        LotStatus status = LotStatus.Available,
        string? certificateUrl = null,
        string? notes = null,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        SetLotNumber(lotNumber);
        ManufactureDate = manufactureDate;
        ExpiryDate = expiryDate;
        SupplierLotNumber = supplierLotNumber?.Trim();
        Status = status;
        CertificateUrl = certificateUrl?.Trim();
        Notes = notes?.Trim();
    }

    public void SetLotNumber(string lotNumber)
    {
        Check.NotNullOrWhiteSpace(lotNumber, nameof(lotNumber), InventoryConsts.MaxLotBatchNumberLength);
        LotNumber = lotNumber.Trim().ToUpperInvariant();
    }
}

