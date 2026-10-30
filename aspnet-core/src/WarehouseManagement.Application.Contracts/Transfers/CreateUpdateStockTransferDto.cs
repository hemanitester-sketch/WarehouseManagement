using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Transfers;

public class CreateStockTransferItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    public Guid? SourceLocationId { get; set; }

    public Guid? DestinationLocationId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Requested quantity must be greater than zero.")]
    public decimal QuantityRequested { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? Lpn { get; set; }
}

public class CreateUpdateStockTransferDto
{
    [Required]
    public TransferType TransferType { get; set; } = TransferType.InterWarehouse;

    [Required]
    public Guid SourceWarehouseId { get; set; }

    [Required]
    public Guid DestinationWarehouseId { get; set; }

    public Guid? SourceLocationId { get; set; }

    public Guid? DestinationLocationId { get; set; }

    [StringLength(TransferConsts.MaxCarrierNameLength)]
    public string? CarrierName { get; set; }

    [StringLength(TransferConsts.MaxTrackingNumberLength)]
    public string? TrackingNumber { get; set; }

    [StringLength(TransferConsts.MaxVehicleNumberLength)]
    public string? VehicleNumber { get; set; }

    [StringLength(TransferConsts.MaxDriverNameLength)]
    public string? DriverName { get; set; }

    [StringLength(TransferConsts.MaxDriverPhoneLength)]
    public string? DriverPhone { get; set; }

    public DateTime? TransferDate { get; set; }

    [StringLength(TransferConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    public List<CreateStockTransferItemDto> Items { get; set; } = new();
}

public class DispatchTransferDto
{
    [StringLength(TransferConsts.MaxCarrierNameLength)]
    public string? CarrierName { get; set; }

    [StringLength(TransferConsts.MaxTrackingNumberLength)]
    public string? TrackingNumber { get; set; }

    [StringLength(TransferConsts.MaxVehicleNumberLength)]
    public string? VehicleNumber { get; set; }

    [StringLength(TransferConsts.MaxDriverNameLength)]
    public string? DriverName { get; set; }

    [StringLength(TransferConsts.MaxDriverPhoneLength)]
    public string? DriverPhone { get; set; }
}
