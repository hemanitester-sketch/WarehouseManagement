namespace WarehouseManagement.Audits;

public enum DiscrepancyReason
{
    None = 0,
    ShrinkageOrTheft = 1,
    MisplacedInWrongBin = 2,
    DamagedOrExpired = 3,
    DataEntryMismatch = 4,
    InboundShortage = 5,
    PickingError = 6,
    Other = 7
}

