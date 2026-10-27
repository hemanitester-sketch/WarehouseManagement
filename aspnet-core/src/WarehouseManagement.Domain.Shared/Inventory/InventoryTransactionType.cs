namespace WarehouseManagement.Inventory;

public enum InventoryTransactionType
{
    InboundReceipt = 1,
    OutboundPick = 2,
    BinTransfer = 3,
    AdjustmentIncrease = 4,
    AdjustmentDecrease = 5,
    DamageWriteOff = 6,
    QuarantineRelease = 7
}
