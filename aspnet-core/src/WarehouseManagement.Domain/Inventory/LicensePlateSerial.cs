using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WarehouseManagement.Products;

namespace WarehouseManagement.Inventory;

public class LicensePlateSerial : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid LicensePlateId { get; set; }

    public Guid ProductVariantId { get; set; }

    public Guid SerialId { get; set; }

    public LicensePlate LicensePlate { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;

    public InventorySerial Serial { get; set; } = null!;

    public LicensePlateSerial()
    {
    }

    public LicensePlateSerial(
        Guid id,
        Guid licensePlateId,
        Guid productVariantId,
        Guid serialId,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        LicensePlateId = licensePlateId;
        ProductVariantId = productVariantId;
        SerialId = serialId;
    }
}

