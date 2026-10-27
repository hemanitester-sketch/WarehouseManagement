using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Inbound;

public class InboundOrder : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public string SupplierName { get; set; } = null!;

    public string? SupplierInvoiceNumber { get; set; }

    public DateTime ExpectedArrivalDate { get; set; }

    public DateTime? ActualArrivalDate { get; set; }

    public InboundOrderStatus Status { get; set; }

    public string? Notes { get; set; }

    public List<InboundOrderLine> Lines { get; set; } = new();

    public int TotalLines => Lines.Count;

    public decimal TotalExpectedQuantity => Lines.Sum(x => x.ExpectedQuantity);

    public decimal TotalReceivedQuantity => Lines.Sum(x => x.ReceivedQuantity);

    public InboundOrder()
    {
    }

    public InboundOrder(
        Guid id,
        string orderNumber,
        Guid warehouseId,
        string supplierName,
        DateTime expectedArrivalDate,
        Guid? tenantId = null,
        string? supplierInvoiceNumber = null,
        InboundOrderStatus status = InboundOrderStatus.Draft,
        string? notes = null)
        : base(id)
    {
        TenantId = tenantId;
        SetOrderNumber(orderNumber);
        WarehouseId = warehouseId;
        SetSupplierName(supplierName);
        ExpectedArrivalDate = expectedArrivalDate;
        SupplierInvoiceNumber = supplierInvoiceNumber?.Trim();
        Status = status;
        Notes = notes?.Trim();
        Lines = new List<InboundOrderLine>();
    }

    public void SetOrderNumber(string orderNumber)
    {
        Check.NotNullOrWhiteSpace(orderNumber, nameof(orderNumber), InboundConsts.MaxOrderNumberLength);
        OrderNumber = orderNumber.Trim().ToUpperInvariant();
    }

    public void SetSupplierName(string supplierName)
    {
        Check.NotNullOrWhiteSpace(supplierName, nameof(supplierName), InboundConsts.MaxSupplierNameLength);
        SupplierName = supplierName.Trim();
    }

    public InboundOrderLine AddLine(Guid id, Guid productId, decimal expectedQuantity, decimal unitPrice = 0)
    {
        if (expectedQuantity <= 0)
        {
            throw new UserFriendlyException("Expected quantity must be greater than zero.");
        }

        var line = new InboundOrderLine(
            id,
            Id,
            productId,
            expectedQuantity,
            unitPrice,
            TenantId
        );

        Lines.Add(line);
        return line;
    }

    public void ReceiveItem(Guid lineId, decimal quantity)
    {
        var line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new UserFriendlyException("Inbound order line not found.");
        }

        line.AddReceivedQuantity(quantity);

        if (ActualArrivalDate == null)
        {
            ActualArrivalDate = DateTime.UtcNow;
        }

        // Auto calculate status
        if (Lines.All(x => x.IsCompleted))
        {
            Status = InboundOrderStatus.Completed;
        }
        else if (Lines.Any(x => x.ReceivedQuantity > 0))
        {
            Status = InboundOrderStatus.PartiallyReceived;
        }
    }
}
