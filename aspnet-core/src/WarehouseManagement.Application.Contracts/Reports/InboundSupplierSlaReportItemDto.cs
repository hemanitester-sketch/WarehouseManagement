using System;

namespace WarehouseManagement.Reports;

public class InboundSupplierSlaReportItemDto
{
    public Guid? SupplierId { get; set; }

    public string SupplierCode { get; set; } = null!;

    public string SupplierName { get; set; } = null!;

    public int TotalPurchaseOrders { get; set; }

    public int CompletedOrders { get; set; }

    public int PendingOrders { get; set; }

    public decimal TotalExpectedQuantity { get; set; }

    public decimal TotalReceivedQuantity { get; set; }

    public double FulfillmentRatePercentage { get; set; }

    public int OnTimeDeliveries { get; set; }

    public double OnTimeDeliveryPercentage { get; set; }
}
