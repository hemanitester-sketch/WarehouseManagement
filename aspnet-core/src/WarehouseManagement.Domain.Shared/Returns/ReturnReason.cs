namespace WarehouseManagement.Returns;

public enum ReturnReason
{
    DefectiveItem = 1,
    WrongItemShipped = 2,
    DamagedInTransit = 3,
    CustomerCancellation = 4,
    QualityDiscrepancy = 5,
    ExpiredProduct = 6,
    ExcessStock = 7,
    Other = 8
}
