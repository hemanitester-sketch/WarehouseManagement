using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Outbound;

public class OutboundOrder : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? ShippingAddress { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public OrderPriority Priority { get; set; }

    public OutboundOrderStatus Status { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime? RequiredDeliveryDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public string? CarrierName { get; set; }

    public string? TrackingNumber { get; set; }

    public string? Notes { get; set; }

    public List<OutboundOrderLine> Lines { get; set; } = new();

    public int TotalLines => Lines.Count;

    public decimal TotalOrderedQuantity => Lines.Sum(x => x.OrderedQuantity);

    public decimal TotalAllocatedQuantity => Lines.Sum(x => x.AllocatedQuantity);

    public decimal TotalPickedQuantity => Lines.Sum(x => x.PickedQuantity);

    public OutboundOrder()
    {
    }

    public OutboundOrder(
        Guid id,
        string orderNumber,
        Guid warehouseId,
        string customerName,
        DateTime orderDate,
        Guid? tenantId = null,
        string? shippingAddress = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null,
        OrderPriority priority = OrderPriority.Normal,
        OutboundOrderStatus status = OutboundOrderStatus.Draft,
        DateTime? requiredDeliveryDate = null,
        string? notes = null)
        : base(id)
    {
        TenantId = tenantId;
        SetOrderNumber(orderNumber);
        WarehouseId = warehouseId;
        SetCustomerName(customerName);
        OrderDate = orderDate;
        ShippingAddress = shippingAddress?.Trim();
        City = city?.Trim();
        State = state?.Trim();
        PostalCode = postalCode?.Trim();
        Country = country?.Trim();
        Priority = priority;
        Status = status;
        RequiredDeliveryDate = requiredDeliveryDate;
        Notes = notes?.Trim();
        Lines = new List<OutboundOrderLine>();
    }

    public void SetOrderNumber(string orderNumber)
    {
        Check.NotNullOrWhiteSpace(orderNumber, nameof(orderNumber), OutboundConsts.MaxOrderNumberLength);
        OrderNumber = orderNumber.Trim().ToUpperInvariant();
    }

    public void SetCustomerName(string customerName)
    {
        Check.NotNullOrWhiteSpace(customerName, nameof(customerName), OutboundConsts.MaxCustomerNameLength);
        CustomerName = customerName.Trim();
    }

    public OutboundOrderLine AddLine(Guid id, Guid productId, decimal orderedQuantity, decimal unitPrice = 0)
    {
        if (orderedQuantity <= 0)
        {
            throw new UserFriendlyException("Ordered quantity must be greater than zero.");
        }

        var line = new OutboundOrderLine(
            id,
            Id,
            productId,
            orderedQuantity,
            unitPrice,
            TenantId
        );

        Lines.Add(line);
        return line;
    }

    public void PickItem(Guid lineId, decimal quantity)
    {
        var line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new UserFriendlyException("Outbound order line not found.");
        }

        line.AddPickedQuantity(quantity);

        if (Lines.All(x => x.IsCompleted))
        {
            Status = OutboundOrderStatus.Picked;
        }
        else
        {
            Status = OutboundOrderStatus.Picking;
        }
    }

    public void Ship(string? carrierName, string? trackingNumber)
    {
        if (Status != OutboundOrderStatus.Picked && Status != OutboundOrderStatus.Packed)
        {
            throw new UserFriendlyException("Order can only be shipped once all lines are picked or packed.");
        }

        CarrierName = carrierName?.Trim();
        TrackingNumber = trackingNumber?.Trim();
        ShippedDate = DateTime.UtcNow;
        Status = OutboundOrderStatus.Shipped;
    }
}
