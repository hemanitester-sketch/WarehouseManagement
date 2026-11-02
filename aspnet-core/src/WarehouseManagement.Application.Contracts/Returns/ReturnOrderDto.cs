using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Returns;

public class ReturnOrderDto : FullAuditedEntityDto<Guid>
{
    public string RmaNumber { get; set; } = null!;

    public ReturnType ReturnType { get; set; }

    public string ReturnTypeName => ReturnType.ToString();

    public ReturnOrderStatus Status { get; set; }

    public string StatusName => Status.ToString();

    public Guid WarehouseId { get; set; }

    public string? WarehouseCode { get; set; }

    public string? WarehouseName { get; set; }

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

    public List<ReturnOrderItemDto> Items { get; set; } = new();
}

