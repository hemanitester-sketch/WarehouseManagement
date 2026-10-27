using System;
using System.ComponentModel.DataAnnotations;
using WarehouseManagement.Inventory;

namespace WarehouseManagement.Inbound;

public class ReceiveInboundGoodsDto
{
    [Required]
    public Guid InboundOrderId { get; set; }

    [Required]
    public Guid InboundOrderLineId { get; set; }

    [Required]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Received quantity must be greater than zero.")]
    public decimal ReceivedQuantity { get; set; }

    [Required]
    public Guid DestinationLocationId { get; set; }

    [StringLength(InventoryConsts.MaxLotBatchNumberLength)]
    public string? LotBatchNumber { get; set; }

    [StringLength(InventoryConsts.MaxSerialNumberLength)]
    public string? SerialNumber { get; set; }

    [StringLength(InventoryConsts.MaxLpnLength)]
    public string? Lpn { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime? ManufactureDate { get; set; }

    [StringLength(InboundConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}
