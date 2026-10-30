using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Transfers;

public class StockTransferDto : FullAuditedEntityDto<Guid>
{
    public string TransferNumber { get; set; } = null!;

    public TransferType TransferType { get; set; }

    public string TransferTypeLabel { get; set; } = null!;

    public TransferStatus Status { get; set; }

    public string StatusLabel { get; set; } = null!;

    public Guid SourceWarehouseId { get; set; }

    public string? SourceWarehouseCode { get; set; }

    public string? SourceWarehouseName { get; set; }

    public Guid DestinationWarehouseId { get; set; }

    public string? DestinationWarehouseCode { get; set; }

    public string? DestinationWarehouseName { get; set; }

    public Guid? SourceLocationId { get; set; }

    public string? SourceLocationBarcode { get; set; }

    public Guid? DestinationLocationId { get; set; }

    public string? DestinationLocationBarcode { get; set; }

    public string? CarrierName { get; set; }

    public string? TrackingNumber { get; set; }

    public string? VehicleNumber { get; set; }

    public string? DriverName { get; set; }

    public string? DriverPhone { get; set; }

    public DateTime TransferDate { get; set; }

    public DateTime? DispatchedDate { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public string? Notes { get; set; }

    public int TotalItemsCount { get; set; }

    public decimal TotalQuantity { get; set; }

    public List<StockTransferItemDto> Items { get; set; } = new();
}

