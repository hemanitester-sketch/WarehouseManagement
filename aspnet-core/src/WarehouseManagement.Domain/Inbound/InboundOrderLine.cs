using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using WarehouseManagement.Products;

namespace WarehouseManagement.Inbound;

public class InboundOrderLine : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid InboundOrderId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariantId { get; set; }

    public decimal ExpectedQuantity { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsCompleted => ReceivedQuantity >= ExpectedQuantity;

    public Product Product { get; set; } = null!;

    public ProductVariant? ProductVariant { get; set; }

    public InboundOrderLine()
    {
    }

    public InboundOrderLine(
        Guid id,
        Guid inboundOrderId,
        Guid productId,
        decimal expectedQuantity,
        decimal unitPrice = 0,
        Guid? tenantId = null,
        decimal receivedQuantity = 0,
        Guid? productVariantId = null)
        : base(id)
    {
        InboundOrderId = inboundOrderId;
        ProductId = productId;
        ExpectedQuantity = Math.Max(0, expectedQuantity);
        UnitPrice = unitPrice;
        TenantId = tenantId;
        ReceivedQuantity = Math.Max(0, receivedQuantity);
        ProductVariantId = productVariantId;
    }

    public void AddReceivedQuantity(decimal quantity)
    {
        ReceivedQuantity += quantity;
    }
}
