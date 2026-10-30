namespace WarehouseManagement.Transfers;

public enum TransferType
{
    InterWarehouse = 1,   // Multi-facility truck transfer
    BinToBin = 2,         // Internal location relocation
    Replenishment = 3,    // Reserve rack to pick face
    Quarantine = 4        // Move to quality inspection / damaged zone
}
