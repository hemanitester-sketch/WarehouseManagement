using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Warehouses;

public class WarehouseLocationDto : FullAuditedEntityDto<Guid>
{
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
}
