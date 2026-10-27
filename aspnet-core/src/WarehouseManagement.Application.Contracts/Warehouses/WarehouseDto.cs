using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Warehouses;

public class WarehouseDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public WarehouseType Type { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? ContactPerson { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public double TotalAreaSqFt { get; set; }

    public bool IsActive { get; set; }
}
