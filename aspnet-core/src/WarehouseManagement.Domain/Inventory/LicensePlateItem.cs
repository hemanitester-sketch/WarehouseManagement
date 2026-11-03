using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WarehouseManagement.Products;

namespace WarehouseManagement.Inventory;

public class LicensePlateItem : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid LicensePlateId { get; set; }

    public Guid ProductVariantId { get; set; }

    public Guid? LotId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public UnitOfMeasure Uom { get; set; } = UnitOfMeasure.Pieces;

    public LicensePlate LicensePlate { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;

    public InventoryLot? Lot { get; set; }

    public LicensePlateItem()
    {
    }

    public LicensePlateItem(
        Guid id,
        Guid licensePlateId,
        Guid productVariantId,
        decimal quantityOnHand,
        Guid? lotId = null,
        UnitOfMeasure uom = UnitOfMeasure.Pieces,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        LicensePlateId = licensePlateId;
        ProductVariantId = productVariantId;
        LotId = lotId;
        Uom = uom;
        SetQuantity(quantityOnHand);
    }

    public void SetQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new UserFriendlyException($"License plate item quantity must be strictly greater than zero. Received: {quantity}");
        }
        QuantityOnHand = quantity;
    }

    public void AdjustQuantity(decimal delta)
    {
        var newQty = QuantityOnHand + delta;
        if (newQty < 0)
        {
            throw new UserFriendlyException($"Cannot reduce LPN item quantity below zero. Current: {QuantityOnHand}, delta: {delta}");
        }
        QuantityOnHand = newQty;
    }
}

