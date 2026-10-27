using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Outbound;

public class AllocateOrderStockDto
{
    [Required]
    public Guid OutboundOrderId { get; set; }
}
