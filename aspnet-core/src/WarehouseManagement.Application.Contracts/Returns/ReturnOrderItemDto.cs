using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Returns;

public class ReturnOrderItemDto : FullAuditedEntityDto<Guid>
{
    public Guid ReturnOrderId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductSku { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public decimal ExpectedQuantity { get; set; }

    public decimal ReceivedQuantity { get; set; }

    public decimal RestockedQuantity { get; set; }

    public decimal ScrappedQuantity { get; set; }

    public decimal UnitRefundPrice { get; set; }

    public ReturnReason ReturnReason { get; set; }

    public string ReturnReasonName => ReturnReason.ToString();

    public ItemConditionGrade? ConditionGrade { get; set; }

    public string? ConditionGradeName => ConditionGrade?.ToString();

    public ItemResolutionAction ResolutionAction { get; set; }

    public string ResolutionActionName => ResolutionAction.ToString();

    public Guid? DestinationLocationId { get; set; }

    public string? DestinationLocationBarcode { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? InspectionNotes { get; set; }
}
