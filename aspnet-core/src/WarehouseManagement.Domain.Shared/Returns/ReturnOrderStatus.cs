namespace WarehouseManagement.Returns;

public enum ReturnOrderStatus
{
    Draft = 1,
    Requested = 2,
    Approved = 3,
    InTransit = 4,
    Received = 5,
    UnderInspection = 6,
    Completed = 7,
    Rejected = 8,
    Cancelled = 9
}
