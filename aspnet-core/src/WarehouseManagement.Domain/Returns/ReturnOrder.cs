using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Returns;

public class ReturnOrder : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string RmaNumber { get; set; } = null!;

    public ReturnType ReturnType { get; set; }

    public ReturnOrderStatus Status { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public Guid? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public Guid? OutboundOrderId { get; set; }

    public string? OutboundOrderNumber { get; set; }

    public Guid? InboundOrderId { get; set; }

    public string? InboundOrderNumber { get; set; }

    public string? CarrierName { get; set; }

    public string? TrackingNumber { get; set; }

    public DateTime ReturnDate { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public DateTime? InspectedDate { get; set; }

    public string? InspectedBy { get; set; }

    public decimal TotalQuantityExpected { get; set; }

    public decimal TotalQuantityReceived { get; set; }

    public decimal TotalQuantityRestocked { get; set; }

    public decimal TotalQuantityScrapped { get; set; }

    public decimal TotalRefundOrCreditAmount { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<ReturnOrderItem> Items { get; set; } = new List<ReturnOrderItem>();

    public ReturnOrder()
    {
    }

    public ReturnOrder(
        Guid id,
        string rmaNumber,
        ReturnType returnType,
        Guid warehouseId,
        Guid? customerId = null,
        string? customerName = null,
        Guid? supplierId = null,
        string? supplierName = null,
        Guid? outboundOrderId = null,
        string? outboundOrderNumber = null,
        Guid? inboundOrderId = null,
        string? inboundOrderNumber = null,
        string? carrierName = null,
        string? trackingNumber = null,
        DateTime? returnDate = null,
        string? notes = null,
        Guid? tenantId = null)
        : base(id)
    {
        RmaNumber = Check.NotNullOrWhiteSpace(rmaNumber, nameof(rmaNumber), RmaConsts.MaxRmaNumberLength);
        ReturnType = returnType;
        Status = ReturnOrderStatus.Draft;
        WarehouseId = warehouseId;
        CustomerId = customerId;
        CustomerName = customerName?.Trim();
        SupplierId = supplierId;
        SupplierName = supplierName?.Trim();
        OutboundOrderId = outboundOrderId;
        OutboundOrderNumber = outboundOrderNumber?.Trim();
        InboundOrderId = inboundOrderId;
        InboundOrderNumber = inboundOrderNumber?.Trim();
        CarrierName = carrierName?.Trim();
        TrackingNumber = trackingNumber?.Trim();
        ReturnDate = returnDate ?? DateTime.UtcNow;
        Notes = notes?.Trim();
        TenantId = tenantId;
        Items = new List<ReturnOrderItem>();
    }

    public void AddItem(
        Guid productId,
        string productSku,
        string productName,
        decimal expectedQuantity,
        decimal unitRefundPrice = 0,
        ReturnReason returnReason = ReturnReason.DefectiveItem,
        string? lotBatchNumber = null,
        string? serialNumber = null)
    {
        if (expectedQuantity <= 0)
        {
            throw new UserFriendlyException("Expected return quantity must be greater than zero.");
        }

        var item = new ReturnOrderItem(
            Guid.NewGuid(),
            Id,
            productId,
            productSku,
            productName,
            expectedQuantity,
            unitRefundPrice,
            returnReason,
            lotBatchNumber,
            serialNumber);

        Items.Add(item);
        RecalculateTotals();
    }

    public void Approve()
    {
        if (Status != ReturnOrderStatus.Draft && Status != ReturnOrderStatus.Requested)
        {
            throw new UserFriendlyException($"Cannot approve return with status '{Status}'.");
        }

        Status = ReturnOrderStatus.Approved;
    }

    public void MarkInTransit(string? carrierName, string? trackingNumber)
    {
        CarrierName = carrierName?.Trim() ?? CarrierName;
        TrackingNumber = trackingNumber?.Trim() ?? TrackingNumber;
        Status = ReturnOrderStatus.InTransit;
    }

    public void MarkReceived(DateTime? receivedDate = null, string? notes = null)
    {
        ReceivedDate = receivedDate ?? DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? notes : $"{Notes} | {notes}";
        }
        Status = ReturnOrderStatus.Received;
    }

    public void StartInspection(string? inspectorName)
    {
        InspectedBy = inspectorName?.Trim() ?? InspectedBy;
        Status = ReturnOrderStatus.UnderInspection;
    }

    public void Complete(string? inspectorName = null)
    {
        InspectedDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(inspectorName))
        {
            InspectedBy = inspectorName.Trim();
        }
        Status = ReturnOrderStatus.Completed;
        RecalculateTotals();
    }

    public void Reject(string? reason)
    {
        Status = ReturnOrderStatus.Rejected;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? $"Rejected: {reason}" : $"{Notes} | Rejected: {reason}";
        }
    }

    public void Cancel(string? reason)
    {
        Status = ReturnOrderStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? $"Cancelled: {reason}" : $"{Notes} | Cancelled: {reason}";
        }
    }

    public void RecalculateTotals()
    {
        TotalQuantityExpected = Items.Sum(x => x.ExpectedQuantity);
        TotalQuantityReceived = Items.Sum(x => x.ReceivedQuantity);
        TotalQuantityRestocked = Items.Sum(x => x.RestockedQuantity);
        TotalQuantityScrapped = Items.Sum(x => x.ScrappedQuantity);
        TotalRefundOrCreditAmount = Items.Sum(x => (x.ReceivedQuantity > 0 ? x.ReceivedQuantity : x.ExpectedQuantity) * x.UnitRefundPrice);
    }
}
