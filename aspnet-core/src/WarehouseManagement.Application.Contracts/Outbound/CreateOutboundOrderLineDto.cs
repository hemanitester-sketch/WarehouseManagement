using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Outbound;

public class CreateOutboundOrderLineDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Ordered quantity must be greater than zero.")]
    public decimal OrderedQuantity { get; set; }

    public decimal UnitPrice { get; set; } = 0;
}
