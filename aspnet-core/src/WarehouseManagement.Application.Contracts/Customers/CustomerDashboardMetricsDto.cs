namespace WarehouseManagement.Customers;

public class CustomerDashboardMetricsDto
{
    public int TotalClientsCount { get; set; }

    public int ActiveClientsCount { get; set; }

    public int CorporateB2BCount { get; set; }

    public int RetailDistributorCount { get; set; }

    public decimal TotalCreditLimitExposure { get; set; }

    public decimal TotalCurrentOutstanding { get; set; }

    public decimal AverageCreditUtilizationPercent { get; set; }

    public int OnHoldClientsCount { get; set; }
}

