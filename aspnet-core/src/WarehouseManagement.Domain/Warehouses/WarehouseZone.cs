using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Warehouses;

public class WarehouseZone : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid WarehouseId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public WarehouseZoneStorageType StorageType { get; set; }

    public double? TemperatureMinCelsius { get; set; }

    public double? TemperatureMaxCelsius { get; set; }

    public bool IsActive { get; set; }

    public WarehouseZone()
    {
    }

    public WarehouseZone(
        Guid id,
        Guid warehouseId,
        string code,
        string name,
        WarehouseZoneStorageType storageType,
        Guid? tenantId = null,
        double? temperatureMinCelsius = null,
        double? temperatureMaxCelsius = null,
        bool isActive = true)
        : base(id)
    {
        WarehouseId = warehouseId;
        TenantId = tenantId;
        SetCode(code);
        SetName(name);
        StorageType = storageType;
        TemperatureMinCelsius = temperatureMinCelsius;
        TemperatureMaxCelsius = temperatureMaxCelsius;
        IsActive = isActive;
    }

    public void SetCode(string code)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code), WarehouseLocationConsts.MaxZoneCodeLength);
        Code = code.Trim().ToUpperInvariant();
    }

    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), WarehouseLocationConsts.MaxZoneNameLength);
        Name = name.Trim();
    }
}
