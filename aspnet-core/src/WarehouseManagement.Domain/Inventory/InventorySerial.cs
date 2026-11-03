using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WarehouseManagement.Products;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Inventory;

public class InventorySerial : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid ProductId { get; set; }

    public Guid ProductVariantId { get; set; }

    public string SerialNumber { get; set; } = null!;

    public Guid CurrentWarehouseId { get; set; }

    public Guid? CurrentLocationId { get; set; }

    public Guid? CurrentLicensePlateId { get; set; }

    public SerialStatus Status { get; set; } = SerialStatus.InStock;

    public DateTime ReceivedDate { get; set; }

    public DateTime? WarrantyExpiryDate { get; set; }

    public Product Product { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;

    public Warehouse CurrentWarehouse { get; set; } = null!;

    public WarehouseLocation? CurrentLocation { get; set; }

    public LicensePlate? CurrentLicensePlate { get; set; }

    public InventorySerial()
    {
    }

    public InventorySerial(
        Guid id,
        Guid productId,
        Guid productVariantId,
        string serialNumber,
        Guid currentWarehouseId,
        Guid? currentLocationId = null,
        Guid? currentLicensePlateId = null,
        SerialStatus status = SerialStatus.InStock,
        DateTime? receivedDate = null,
        DateTime? warrantyExpiryDate = null,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        SetSerialNumber(serialNumber);
        CurrentWarehouseId = currentWarehouseId;
        CurrentLocationId = currentLocationId;
        CurrentLicensePlateId = currentLicensePlateId;
        Status = status;
        ReceivedDate = receivedDate ?? DateTime.UtcNow;
        WarrantyExpiryDate = warrantyExpiryDate;
    }

    public void SetSerialNumber(string serialNumber)
    {
        Check.NotNullOrWhiteSpace(serialNumber, nameof(serialNumber), InventoryConsts.MaxSerialNumberLength);
        SerialNumber = serialNumber.Trim().ToUpperInvariant();
    }

    public void Relocate(Guid warehouseId, Guid? locationId, Guid? licensePlateId = null)
    {
        CurrentWarehouseId = warehouseId;
        CurrentLocationId = locationId;
        CurrentLicensePlateId = licensePlateId;
    }

    public void SetStatus(SerialStatus status)
    {
        Status = status;
    }
}

