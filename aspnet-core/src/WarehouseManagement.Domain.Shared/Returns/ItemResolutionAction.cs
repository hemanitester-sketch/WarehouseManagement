namespace WarehouseManagement.Returns;

public enum ItemResolutionAction
{
    PendingInspection = 1,
    RestockToInventory = 2,
    ScrapAndWriteOff = 3,
    ReturnToVendor = 4,
    QuarantineHold = 5
}
