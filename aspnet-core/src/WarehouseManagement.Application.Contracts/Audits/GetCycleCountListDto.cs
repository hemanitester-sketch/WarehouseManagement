using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Audits;

public class GetCycleCountListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid? ZoneId { get; set; }

    public CycleCountType? CountType { get; set; }

    public CycleCountStatus? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}

public class CycleCountDashboardMetricsDto
{
    public int TotalAudits { get; set; }

    public int InProgressCounts { get; set; }

    public int AuditsUnderReview { get; set; }

    public int ReconciledAudits { get; set; }

    public decimal OverallAccuracyRate { get; set; }

    public decimal TotalNetVarianceUnits { get; set; }

    public decimal TotalNetVarianceValue { get; set; }
}

