using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Transfers;

public class StockTransferItemDto : EntityDto<Guid>
{
    public Guid StockTransferId { get; set; }

    public Guid ProductId { get; set; }

    public string? ProductSku { get; set; }

    public string? ProductName { get; set; }

    public Guid? SourceLocationId { get; set; }

    public string? SourceLocationBarcode { get; set; }

    public Guid? DestinationLocationId { get; set; }

    public string? DestinationLocationBarcode { get; set; }

    public decimal QuantityRequested { get; set; }

    public decimal QuantityTransferred { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? Lpn { get; set; }
}

