using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Transfers;

public class StockTransfer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string TransferNumber { get; set; } = null!;

    public TransferType TransferType { get; set; }

    public TransferStatus Status { get; set; }

    public Guid SourceWarehouseId { get; set; }

    public Guid DestinationWarehouseId { get; set; }

    public Guid? SourceLocationId { get; set; }

    public Guid? DestinationLocationId { get; set; }

    public string? CarrierName { get; set; }

    public string? TrackingNumber { get; set; }

    public string? VehicleNumber { get; set; }

    public string? DriverName { get; set; }

    public string? DriverPhone { get; set; }

    public DateTime TransferDate { get; set; }

    public DateTime? DispatchedDate { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();

    public StockTransfer()
    {
    }

    public StockTransfer(
        Guid id,
        string transferNumber,
        TransferType transferType,
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        Guid? tenantId = null,
        Guid? sourceLocationId = null,
        Guid? destinationLocationId = null,
        string? carrierName = null,
        string? trackingNumber = null,
        string? vehicleNumber = null,
        string? driverName = null,
        string? driverPhone = null,
        DateTime? transferDate = null,
        string? notes = null)
        : base(id)
    {
        TransferNumber = Check.NotNullOrWhiteSpace(transferNumber, nameof(transferNumber), TransferConsts.MaxTransferNumberLength);
        TransferType = transferType;
        Status = TransferStatus.Draft;
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        TenantId = tenantId;
        SourceLocationId = sourceLocationId;
        DestinationLocationId = destinationLocationId;
        CarrierName = carrierName?.Trim();
        TrackingNumber = trackingNumber?.Trim();
        VehicleNumber = vehicleNumber?.Trim();
        DriverName = driverName?.Trim();
        DriverPhone = driverPhone?.Trim();
        TransferDate = transferDate ?? DateTime.UtcNow;
        Notes = notes?.Trim();
        Items = new List<StockTransferItem>();
    }

    public void AddItem(
        Guid productId,
        decimal quantityRequested,
        Guid? sourceLocationId = null,
        Guid? destinationLocationId = null,
        decimal? quantityTransferred = null,
        string? lotBatchNumber = null,
        string? serialNumber = null,
        string? lpn = null)
    {
        if (quantityRequested <= 0)
        {
            throw new UserFriendlyException("Transfer item quantity must be greater than zero.");
        }

        var item = new StockTransferItem(
            Guid.NewGuid(),
            Id,
            productId,
            quantityRequested,
            sourceLocationId ?? SourceLocationId,
            destinationLocationId ?? DestinationLocationId,
            quantityTransferred,
            lotBatchNumber,
            serialNumber,
            lpn
        );

        Items.Add(item);
    }

    public void Approve()
    {
        if (Status != TransferStatus.Draft)
        {
            throw new UserFriendlyException($"Only Draft transfer orders can be approved. Current status: {Status}");
        }

        Status = TransferStatus.Approved;
    }

    public void Dispatch(
        string? carrierName = null,
        string? trackingNumber = null,
        string? vehicleNumber = null,
        string? driverName = null,
        string? driverPhone = null)
    {
        if (Status != TransferStatus.Draft && Status != TransferStatus.Approved)
        {
            throw new UserFriendlyException($"Transfer cannot be dispatched from status: {Status}");
        }

        if (!string.IsNullOrWhiteSpace(carrierName)) CarrierName = carrierName.Trim();
        if (!string.IsNullOrWhiteSpace(trackingNumber)) TrackingNumber = trackingNumber.Trim();
        if (!string.IsNullOrWhiteSpace(vehicleNumber)) VehicleNumber = vehicleNumber.Trim();
        if (!string.IsNullOrWhiteSpace(driverName)) DriverName = driverName.Trim();
        if (!string.IsNullOrWhiteSpace(driverPhone)) DriverPhone = driverPhone.Trim();

        DispatchedDate = DateTime.UtcNow;
        Status = TransferStatus.InTransit;
    }

    public void Complete()
    {
        if (Status == TransferStatus.Completed)
        {
            throw new UserFriendlyException("Transfer order is already completed.");
        }

        if (Status == TransferStatus.Cancelled)
        {
            throw new UserFriendlyException("Cannot complete a cancelled transfer order.");
        }

        ReceivedDate = DateTime.UtcNow;
        Status = TransferStatus.Completed;
    }

    public void Cancel(string? reason = null)
    {
        if (Status == TransferStatus.Completed)
        {
            throw new UserFriendlyException("Cannot cancel a completed transfer order.");
        }

        Status = TransferStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? $"[Cancelled]: {reason}" : $"{Notes}\n[Cancelled]: {reason}";
        }
    }
}
