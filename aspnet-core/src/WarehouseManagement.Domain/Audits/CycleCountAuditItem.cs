using System;
using Volo.Abp.Domain.Entities;

namespace WarehouseManagement.Audits;

public class CycleCountAuditItem : Entity<Guid>
{
    public Guid CycleCountAuditId { get; set; }

    public Guid ProductId { get; set; }

    public Guid LocationId { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public decimal SystemQuantity { get; set; }

    public decimal? CountedQuantity { get; set; }

    public decimal VarianceQuantity => (CountedQuantity ?? SystemQuantity) - SystemQuantity;

    public decimal UnitCost { get; set; }

    public decimal VarianceValue => VarianceQuantity * UnitCost;

    public DiscrepancyReason DiscrepancyReason { get; set; }

    public string? ReasonNotes { get; set; }

    public bool IsReCounted { get; set; }

    public DateTime? CountedAt { get; set; }

    public CycleCountAuditItem()
    {
    }

    public CycleCountAuditItem(
        Guid id,
        Guid cycleCountAuditId,
        Guid productId,
        Guid locationId,
        decimal systemQuantity,
        decimal unitCost,
        string? lotBatchNumber = null,
        string? serialNumber = null)
        : base(id)
    {
        CycleCountAuditId = cycleCountAuditId;
        ProductId = productId;
        LocationId = locationId;
        SystemQuantity = systemQuantity;
        UnitCost = unitCost;
        LotBatchNumber = lotBatchNumber;
        SerialNumber = serialNumber;
        DiscrepancyReason = DiscrepancyReason.None;
    }
}

