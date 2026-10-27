using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Warehouses;

public class CreateUpdateWarehouseZoneDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    [StringLength(WarehouseLocationConsts.MaxZoneCodeLength)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(WarehouseLocationConsts.MaxZoneNameLength)]
    public string Name { get; set; } = null!;

    public WarehouseZoneStorageType StorageType { get; set; } = WarehouseZoneStorageType.Ambient;

    public double? TemperatureMinCelsius { get; set; }

    public double? TemperatureMaxCelsius { get; set; }

    public bool IsActive { get; set; } = true;
}
