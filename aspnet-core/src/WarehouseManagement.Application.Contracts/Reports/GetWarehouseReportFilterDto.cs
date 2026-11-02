using System;

namespace WarehouseManagement.Reports;

public class GetWarehouseReportFilterDto
{
    public Guid? WarehouseId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? Filter { get; set; }
}
