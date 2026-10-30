namespace WarehouseManagement.Transfers;

public enum TransferStatus
{
    Draft = 1,          // Created, awaiting confirmation / dispatch
    Approved = 2,       // Approved by supervisor
    InTransit = 3,      // Dispatched / En route on vehicle or trolley
    Completed = 4,      // Arrived & Put Away at destination
    Cancelled = 5       // Voided / Aborted
}

