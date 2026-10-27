using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Outbound;

public class OutboundOrderDto : FullAuditedEntityDto<Guid>
{
    public string OrderNumber { get; set; } = null!;

    public Guid WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }

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

    public int TotalLines { get; set; }
    public decimal TotalOrderedQuantity { get; set; }
    public decimal TotalAllocatedQuantity { get; set; }
    public decimal TotalPickedQuantity { get; set; }

    public List<OutboundOrderLineDto> Lines { get; set; } = new();
}
