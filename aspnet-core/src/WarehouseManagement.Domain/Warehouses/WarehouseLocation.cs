using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Warehouses;

public class WarehouseLocation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid ZoneId { get; set; }

    public string Barcode { get; set; } = null!;

    public string Aisle { get; set; } = null!;

    public string Rack { get; set; } = null!;

    public string Shelf { get; set; } = null!;

    public string Bin { get; set; } = null!;

    public WarehouseLocationType LocationType { get; set; }

    public WarehouseLocationStatus Status { get; set; }

    public double MaxWeightKg { get; set; }

    public double MaxVolumeCm3 { get; set; }

    public double? LengthCm { get; set; }

    public double? WidthCm { get; set; }

    public double? HeightCm { get; set; }

    public bool IsPickFace { get; set; }

    public bool IsActive { get; set; }

    public WarehouseLocation()
    {
    }

    public WarehouseLocation(
        Guid id,
        Guid warehouseId,
        Guid zoneId,
        string barcode,
        string aisle,
        string rack,
        string shelf,
        string bin,
        WarehouseLocationType locationType,
        Guid? tenantId = null,
        WarehouseLocationStatus status = WarehouseLocationStatus.Available,
        double maxWeightKg = 1000,
        double maxVolumeCm3 = 1000000,
        double? lengthCm = null,
        double? widthCm = null,
        double? heightCm = null,
        bool isPickFace = false,
        bool isActive = true)
        : base(id)
    {
        WarehouseId = warehouseId;
        ZoneId = zoneId;
        TenantId = tenantId;
        SetBarcode(barcode);
        SetCoordinates(aisle, rack, shelf, bin);
        LocationType = locationType;
        Status = status;
        MaxWeightKg = maxWeightKg;
        MaxVolumeCm3 = maxVolumeCm3;
        LengthCm = lengthCm;
        WidthCm = widthCm;
        HeightCm = heightCm;
        IsPickFace = isPickFace;
        IsActive = isActive;
    }

    public void SetBarcode(string barcode)
    {
        Check.NotNullOrWhiteSpace(barcode, nameof(barcode), WarehouseLocationConsts.MaxBarcodeLength);
        Barcode = barcode.Trim().ToUpperInvariant();
    }

    public void SetCoordinates(string aisle, string rack, string shelf, string bin)
    {
        Check.NotNullOrWhiteSpace(aisle, nameof(aisle), WarehouseLocationConsts.MaxAisleLength);
        Check.NotNullOrWhiteSpace(rack, nameof(rack), WarehouseLocationConsts.MaxRackLength);
        Check.NotNullOrWhiteSpace(shelf, nameof(shelf), WarehouseLocationConsts.MaxShelfLength);
        Check.NotNullOrWhiteSpace(bin, nameof(bin), WarehouseLocationConsts.MaxBinLength);

        Aisle = aisle.Trim().ToUpperInvariant();
        Rack = rack.Trim().ToUpperInvariant();
        Shelf = shelf.Trim().ToUpperInvariant();
        Bin = bin.Trim().ToUpperInvariant();
    }
}
