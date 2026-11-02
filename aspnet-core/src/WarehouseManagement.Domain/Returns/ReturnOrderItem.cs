using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace WarehouseManagement.Returns;

public class ReturnOrderItem : FullAuditedEntity<Guid>
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

    public ItemConditionGrade? ConditionGrade { get; set; }

    public ItemResolutionAction ResolutionAction { get; set; }

    public Guid? DestinationLocationId { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? InspectionNotes { get; set; }

    public ReturnOrderItem()
    {
    }

    public ReturnOrderItem(
        Guid id,
        Guid returnOrderId,
        Guid productId,
        string productSku,
        string productName,
        decimal expectedQuantity,
        decimal unitRefundPrice = 0,
        ReturnReason returnReason = ReturnReason.DefectiveItem,
        string? lotBatchNumber = null,
        string? serialNumber = null)
        : base(id)
    {
        ReturnOrderId = returnOrderId;
        ProductId = productId;
        ProductSku = productSku;
        ProductName = productName;
        ExpectedQuantity = expectedQuantity;
        UnitRefundPrice = unitRefundPrice;
        ReturnReason = returnReason;
        ReceivedQuantity = 0;
        RestockedQuantity = 0;
        ScrappedQuantity = 0;
        ResolutionAction = ItemResolutionAction.PendingInspection;
        LotBatchNumber = lotBatchNumber;
        SerialNumber = serialNumber;
    }
}
