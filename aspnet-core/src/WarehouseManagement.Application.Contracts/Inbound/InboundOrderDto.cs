using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Inbound;

public class InboundOrderDto : FullAuditedEntityDto<Guid>
{
    public string OrderNumber { get; set; } = null!;

    public Guid WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }

    public string SupplierName { get; set; } = null!;
    public string? SupplierInvoiceNumber { get; set; }

    public DateTime ExpectedArrivalDate { get; set; }
    public DateTime? ActualArrivalDate { get; set; }

    public InboundOrderStatus Status { get; set; }
    public string? Notes { get; set; }

    public int TotalLines { get; set; }
    public decimal TotalExpectedQuantity { get; set; }
    public decimal TotalReceivedQuantity { get; set; }

    public List<InboundOrderLineDto> Lines { get; set; } = new();
}
