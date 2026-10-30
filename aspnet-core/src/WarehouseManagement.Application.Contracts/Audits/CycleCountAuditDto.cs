using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Audits;

public class CycleCountAuditDto : FullAuditedEntityDto<Guid>
{
    public string AuditNumber { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public string WarehouseCode { get; set; } = null!;

    public Guid? ZoneId { get; set; }

    public string? ZoneName { get; set; }

    public CycleCountType CountType { get; set; }

    public CycleCountStatus Status { get; set; }

    public bool IsBlindCount { get; set; }

    public DateTime ScheduledDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public Guid? AssignedToFacultyId { get; set; }

    public string? AssignedAuditorName { get; set; }

    public string? ApprovedByFacultyName { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public decimal TotalVarianceQuantity { get; set; }

    public decimal TotalVarianceValue { get; set; }

    public int TotalItemsCount { get; set; }

    public int CountedItemsCount { get; set; }

    public int DiscrepancyItemsCount { get; set; }

    public decimal AccuracyPercentage { get; set; }

    public string? Notes { get; set; }

    public List<CycleCountAuditItemDto> Items { get; set; } = new();
}

