using System;
using System.Collections.Generic;

namespace WarehouseManagement.Reports;

public class QualityReverseLogisticsReportDto
{
    public int TotalAuditsConducted { get; set; }

    public decimal TotalItemsAudited { get; set; }

    public decimal NetVarianceUnits { get; set; }

    public decimal NetVarianceCostValue { get; set; }

    public double AuditAccuracyRatePercentage { get; set; }

    public int TotalRmaReturns { get; set; }

    public decimal TotalReturnedUnits { get; set; }

    public decimal TotalRefundValue { get; set; }

    public decimal TotalRestockedUnits { get; set; }

    public decimal TotalScrappedUnits { get; set; }

    public double RestockRecoveryRatePercentage { get; set; }

    public List<ReasonBreakdownDto> TopReturnReasons { get; set; } = new();
}

public class ReasonBreakdownDto
{
    public string Reason { get; set; } = null!;

    public int Count { get; set; }

    public double Percentage { get; set; }
}
