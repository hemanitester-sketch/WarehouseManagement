using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Outbound;

public class PickOrderGoodsDto
{
    [Required]
    public Guid OutboundOrderId { get; set; }

    [Required]
    public Guid OutboundOrderLineId { get; set; }

    [Required]
    public Guid SourceInventoryItemId { get; set; }

    [Required]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Picked quantity must be greater than zero.")]
    public decimal PickedQuantity { get; set; }

    [StringLength(OutboundConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}
