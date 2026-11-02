using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Returns;

public class CreateReturnOrderDto
{
    [Required]
    public ReturnType ReturnType { get; set; } = ReturnType.CustomerReturn;

    [Required]
    public Guid WarehouseId { get; set; }

    public Guid? CustomerId { get; set; }

    [StringLength(RmaConsts.MaxCustomerNameLength)]
    public string? CustomerName { get; set; }

    public Guid? SupplierId { get; set; }

    [StringLength(RmaConsts.MaxSupplierNameLength)]
    public string? SupplierName { get; set; }

    public Guid? OutboundOrderId { get; set; }

    [StringLength(RmaConsts.MaxOrderNumberLength)]
    public string? OutboundOrderNumber { get; set; }

    public Guid? InboundOrderId { get; set; }

    [StringLength(RmaConsts.MaxOrderNumberLength)]
    public string? InboundOrderNumber { get; set; }

    [StringLength(RmaConsts.MaxCarrierNameLength)]
    public string? CarrierName { get; set; }

    [StringLength(RmaConsts.MaxTrackingNumberLength)]
    public string? TrackingNumber { get; set; }

    public DateTime? ReturnDate { get; set; }

    [StringLength(RmaConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    public List<CreateReturnOrderItemDto> Items { get; set; } = new();
}

