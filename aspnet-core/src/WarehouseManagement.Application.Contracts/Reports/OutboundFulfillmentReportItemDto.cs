using System;
using System.Collections.Generic;

namespace WarehouseManagement.Reports;

public class OutboundFulfillmentReportItemDto
{
    public int TotalOutboundOrders { get; set; }

    public int ShippedOrders { get; set; }

    public int PendingOrders { get; set; }

    public int CancelledOrders { get; set; }

    public decimal TotalUnitsShipped { get; set; }

    public double FulfillmentRatePercentage { get; set; }

    public double OnTimeDispatchPercentage { get; set; }

    public List<CustomerFulfillmentSummaryDto> TopCustomers { get; set; } = new();
}

public class CustomerFulfillmentSummaryDto
{
    public Guid? CustomerId { get; set; }

    public string CustomerCode { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public int TotalOrders { get; set; }

    public decimal TotalUnitsDelivered { get; set; }

    public decimal TotalOrderValue { get; set; }
}

