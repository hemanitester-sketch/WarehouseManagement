using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Audits;

public class CreateCycleCountAuditDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    public Guid? ZoneId { get; set; }

    public Guid? SpecificProductId { get; set; }

    [Required]
    public CycleCountType CountType { get; set; }

    public bool IsBlindCount { get; set; } = true;

    [Required]
    public DateTime ScheduledDate { get; set; } = DateTime.UtcNow;

    public Guid? AssignedToFacultyId { get; set; }

    public string? AssignedAuditorName { get; set; }

    [StringLength(AuditConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    public List<CreateCycleCountAuditItemDto> Items { get; set; } = new();
}

public class CreateCycleCountAuditItemDto
{
    public Guid ProductId { get; set; }

    public Guid LocationId { get; set; }

    public string? LotBatchNumber { get; set; }

    public string? SerialNumber { get; set; }

    public decimal SystemQuantity { get; set; }

    public decimal UnitCost { get; set; }
}

