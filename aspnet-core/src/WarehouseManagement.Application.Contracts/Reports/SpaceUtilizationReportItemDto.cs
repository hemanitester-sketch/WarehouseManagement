using System;
using System.Collections.Generic;

namespace WarehouseManagement.Reports;

public class SpaceUtilizationReportItemDto
{
    public Guid WarehouseId { get; set; }

    public string WarehouseCode { get; set; } = null!;

    public string WarehouseName { get; set; } = null!;

    public string? City { get; set; }

    public int TotalZones { get; set; }

    public int TotalBins { get; set; }

    public int OccupiedBins { get; set; }

    public int AvailableBins { get; set; }

    public double UtilizationPercentage { get; set; }

    public decimal TotalStockUnitsStored { get; set; }

    public List<ZoneUtilizationSummaryDto> ZoneBreakdown { get; set; } = new();
}

public class ZoneUtilizationSummaryDto
{
    public Guid ZoneId { get; set; }

    public string ZoneName { get; set; } = null!;

    public string StorageType { get; set; } = null!;

    public int TotalBins { get; set; }

    public int OccupiedBins { get; set; }

    public double UtilizationPercentage { get; set; }
}
