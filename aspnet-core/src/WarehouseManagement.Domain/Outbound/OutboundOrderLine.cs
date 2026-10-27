using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Outbound;

public class OutboundOrderLine : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid OutboundOrderId { get; set; }

    public Guid ProductId { get; set; }

    public decimal OrderedQuantity { get; set; }

    public decimal AllocatedQuantity { get; set; }

    public decimal PickedQuantity { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsCompleted => PickedQuantity >= OrderedQuantity;

    public OutboundOrderLine()
    {
    }

    public OutboundOrderLine(
        Guid id,
        Guid outboundOrderId,
        Guid productId,
        decimal orderedQuantity,
        decimal unitPrice = 0,
        Guid? tenantId = null,
        decimal allocatedQuantity = 0,
        decimal pickedQuantity = 0)
        : base(id)
    {
        OutboundOrderId = outboundOrderId;
        ProductId = productId;
        OrderedQuantity = Math.Max(0, orderedQuantity);
        UnitPrice = unitPrice;
        TenantId = tenantId;
        AllocatedQuantity = Math.Max(0, allocatedQuantity);
        PickedQuantity = Math.Max(0, pickedQuantity);
    }

    public void AddAllocatedQuantity(decimal quantity)
    {
        AllocatedQuantity += quantity;
    }

    public void AddPickedQuantity(decimal quantity)
    {
        PickedQuantity += quantity;
    }
}
