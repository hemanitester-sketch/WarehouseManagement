using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Outbound;

public class CreateUpdateOutboundOrderDto
{
    [Required]
    [StringLength(OutboundConsts.MaxOrderNumberLength)]
    public string OrderNumber { get; set; } = null!;

    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    [StringLength(OutboundConsts.MaxCustomerNameLength)]
    public string CustomerName { get; set; } = null!;

    [StringLength(OutboundConsts.MaxAddressLength)]
    public string? ShippingAddress { get; set; }

    [StringLength(OutboundConsts.MaxCityLength)]
    public string? City { get; set; }

    [StringLength(OutboundConsts.MaxStateLength)]
    public string? State { get; set; }

    [StringLength(OutboundConsts.MaxPostalCodeLength)]
    public string? PostalCode { get; set; }

    [StringLength(OutboundConsts.MaxCountryLength)]
    public string? Country { get; set; }

    public OrderPriority Priority { get; set; } = OrderPriority.Normal;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? RequiredDeliveryDate { get; set; }

    [StringLength(OutboundConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    public List<CreateOutboundOrderLineDto> Lines { get; set; } = new();
}
