namespace WarehouseManagement.Returns;

public class ReturnDashboardMetricsDto
{
    public int TotalReturns { get; set; }

    public int PendingApprovalCount { get; set; }

    public int InTransitCount { get; set; }

    public int AwaitingInspectionCount { get; set; }

    public int CompletedCount { get; set; }

    public decimal TotalRestockedUnits { get; set; }

    public decimal TotalScrappedUnits { get; set; }

    public decimal TotalRefundValue { get; set; }

    public double RecoveryRatePercentage { get; set; }
}

