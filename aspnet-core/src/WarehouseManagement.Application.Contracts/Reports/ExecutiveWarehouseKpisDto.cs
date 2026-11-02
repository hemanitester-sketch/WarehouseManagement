using System;

namespace WarehouseManagement.Reports;

public class ExecutiveWarehouseKpisDto
{
    public decimal TotalInventoryValuation { get; set; }

    public decimal TotalStockUnits { get; set; }

    public int TotalActiveSkus { get; set; }

    public int LowStockSkuCount { get; set; }

    public double OverallSpaceUtilizationRate { get; set; }

    public int TotalOccupiedBins { get; set; }

    public int TotalWarehouseLocations { get; set; }

    public decimal InboundThroughput30Days { get; set; }

    public decimal OutboundThroughput30Days { get; set; }

    public double AuditAccuracyRate { get; set; }

    public double ReturnRatePercentage { get; set; }

    public int PendingInboundOrders { get; set; }

    public int PendingOutboundOrders { get; set; }

    public decimal TotalSalvageOrScrapValue { get; set; }
}

