using System;
using Volo.Abp.Domain.Entities;

namespace WarehouseManagement.Transfers;

public class StockTransferItem : Entity<Guid>
{
    public Guid StockTransferId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? SourceLocationId { get; set; }

    public Guid? DestinationLocationId { get; set; }

    public decimal QuantityRequested { get; set; }

    public decimal QuantityTransferred { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? Lpn { get; set; }

    public StockTransferItem()
    {
    }

    public StockTransferItem(
        Guid id,
        Guid stockTransferId,
        Guid productId,
        decimal quantityRequested,
        Guid? sourceLocationId = null,
        Guid? destinationLocationId = null,
        decimal? quantityTransferred = null,
        string? lotBatchNumber = null,
        string? serialNumber = null,
        string? lpn = null)
        : base(id)
    {
        StockTransferId = stockTransferId;
        ProductId = productId;
        QuantityRequested = quantityRequested;
        QuantityTransferred = quantityTransferred ?? quantityRequested;
        SourceLocationId = sourceLocationId;
        DestinationLocationId = destinationLocationId;
        LotBatchNumber = lotBatchNumber?.Trim();
        SerialNumber = serialNumber?.Trim();
        Lpn = lpn?.Trim();
    }
}

