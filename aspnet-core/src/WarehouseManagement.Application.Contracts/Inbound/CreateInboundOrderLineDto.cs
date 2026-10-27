using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Inbound;

public class CreateInboundOrderLineDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Expected quantity must be greater than zero.")]
    public decimal ExpectedQuantity { get; set; }

    public decimal UnitPrice { get; set; } = 0;
}
