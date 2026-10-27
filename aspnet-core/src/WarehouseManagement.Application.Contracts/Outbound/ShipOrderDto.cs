using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Outbound;

public class ShipOrderDto
{
    [Required]
    public Guid OutboundOrderId { get; set; }

    [StringLength(OutboundConsts.MaxCarrierNameLength)]
    public string? CarrierName { get; set; }

    [StringLength(OutboundConsts.MaxTrackingNumberLength)]
    public string? TrackingNumber { get; set; }
}
