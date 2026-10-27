using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Inbound;

public class CreateUpdateInboundOrderDto
{
    [Required]
    [StringLength(InboundConsts.MaxOrderNumberLength)]
    public string OrderNumber { get; set; } = null!;

    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    [StringLength(InboundConsts.MaxSupplierNameLength)]
    public string SupplierName { get; set; } = null!;

    [StringLength(InboundConsts.MaxSupplierInvoiceNumberLength)]
    public string? SupplierInvoiceNumber { get; set; }

    public DateTime ExpectedArrivalDate { get; set; } = DateTime.UtcNow.AddDays(1);

    public InboundOrderStatus Status { get; set; } = InboundOrderStatus.Draft;

    [StringLength(InboundConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    public List<CreateInboundOrderLineDto> Lines { get; set; } = new();
}
