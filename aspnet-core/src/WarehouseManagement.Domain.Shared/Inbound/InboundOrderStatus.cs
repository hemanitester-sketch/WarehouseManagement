namespace WarehouseManagement.Inbound;

public enum InboundOrderStatus
{
    Draft = 1,
    InTransit = 2,
    Arrived = 3,
    PartiallyReceived = 4,
    Completed = 5,
    Cancelled = 6
}
