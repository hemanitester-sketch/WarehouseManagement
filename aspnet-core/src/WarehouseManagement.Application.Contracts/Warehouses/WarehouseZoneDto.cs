using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Warehouses;

public class WarehouseZoneDto : FullAuditedEntityDto<Guid>
{
    public Guid WarehouseId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public WarehouseZoneStorageType StorageType { get; set; }

    public double? TemperatureMinCelsius { get; set; }

    public double? TemperatureMaxCelsius { get; set; }

    public bool IsActive { get; set; }
}
