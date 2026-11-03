using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Inventory;

public class LicensePlate : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string LpnCode { get; set; } = null!;

    public LicensePlateType Type { get; set; } = LicensePlateType.Pallet;

    public LicensePlateStatus Status { get; set; } = LicensePlateStatus.Empty;

    public Guid CurrentWarehouseId { get; set; }

    public Guid? CurrentLocationId { get; set; }

    public decimal TareWeightKg { get; set; }

    public decimal MaxWeightKg { get; set; }

    public Warehouse CurrentWarehouse { get; set; } = null!;

    public WarehouseLocation? CurrentLocation { get; set; }

    public ICollection<LicensePlateItem> Items { get; set; } = new List<LicensePlateItem>();

    public ICollection<LicensePlateSerial> Serials { get; set; } = new List<LicensePlateSerial>();

    public LicensePlate()
    {
    }

    public LicensePlate(
        Guid id,
        string lpnCode,
        Guid currentWarehouseId,
        Guid? currentLocationId = null,
        LicensePlateType type = LicensePlateType.Pallet,
        LicensePlateStatus status = LicensePlateStatus.Empty,
        decimal tareWeightKg = 0,
        decimal maxWeightKg = 1000,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        SetLpnCode(lpnCode);
        CurrentWarehouseId = currentWarehouseId;
        CurrentLocationId = currentLocationId;
        Type = type;
        Status = status;
        TareWeightKg = Math.Max(0, tareWeightKg);
        MaxWeightKg = Math.Max(0, maxWeightKg);
    }

    public void SetLpnCode(string lpnCode)
    {
        Check.NotNullOrWhiteSpace(lpnCode, nameof(lpnCode), InventoryConsts.MaxLpnLength);
        LpnCode = lpnCode.Trim().ToUpperInvariant();
    }

    public void MoveToLocation(Guid warehouseId, Guid? locationId)
    {
        CurrentWarehouseId = warehouseId;
        CurrentLocationId = locationId;
    }

    public void SetStatus(LicensePlateStatus status)
    {
        Status = status;
    }
}

