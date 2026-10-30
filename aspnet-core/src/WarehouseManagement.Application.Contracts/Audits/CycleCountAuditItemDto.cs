using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Audits;

public class CycleCountAuditItemDto : EntityDto<Guid>
{
    public Guid CycleCountAuditId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string ProductSku { get; set; } = null!;

    public string? ProductBarcode { get; set; }

    public string? CategoryName { get; set; }

    public Guid LocationId { get; set; }

    public string LocationCode { get; set; } = null!;

    public string? ZoneName { get; set; }

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
}

