using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Returns;

public class CreateReturnOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Expected quantity must be greater than zero.")]
    public decimal ExpectedQuantity { get; set; }

    public decimal UnitRefundPrice { get; set; } = 0;

    public ReturnReason ReturnReason { get; set; } = ReturnReason.DefectiveItem;

    [StringLength(RmaConsts.MaxLotBatchNumberLength)]
    public string? LotBatchNumber { get; set; }

    [StringLength(RmaConsts.MaxSerialNumberLength)]
    public string? SerialNumber { get; set; }
}
