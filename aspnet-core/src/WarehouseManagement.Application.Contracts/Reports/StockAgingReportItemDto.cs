using System;

namespace WarehouseManagement.Reports;

public class StockAgingReportItemDto
{
    public Guid ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public decimal QuantityOnHand { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalValuation { get; set; }

    public DateTime? OldestReceivedDate { get; set; }

    public int AgeInDays { get; set; }

    public string AgeBracket { get; set; } = null!; // "0-30 Days", "31-60 Days", "61-90 Days", "90+ Days"

    public string FsnClassification { get; set; } = null!; // "Fast-Moving (F)", "Slow-Moving (S)", "Non-Moving (N)"

    public decimal OutboundShippedLast90Days { get; set; }
}
