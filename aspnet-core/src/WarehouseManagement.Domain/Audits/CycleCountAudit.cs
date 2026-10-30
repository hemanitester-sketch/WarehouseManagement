using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Audits;

public class CycleCountAudit : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string AuditNumber { get; set; } = null!;

    public Guid WarehouseId { get; set; }

    public Guid? ZoneId { get; set; }

    public CycleCountType CountType { get; set; }

    public CycleCountStatus Status { get; set; }

    public bool IsBlindCount { get; set; }

    public DateTime ScheduledDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public Guid? AssignedToFacultyId { get; set; }

    public string? AssignedAuditorName { get; set; }

    public string? ApprovedByFacultyName { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public decimal TotalVarianceQuantity { get; set; }

    public decimal TotalVarianceValue { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<CycleCountAuditItem> Items { get; set; } = new List<CycleCountAuditItem>();

    public CycleCountAudit()
    {
    }

    public CycleCountAudit(
        Guid id,
        string auditNumber,
        Guid warehouseId,
        CycleCountType countType,
        DateTime scheduledDate,
        bool isBlindCount = false,
        Guid? zoneId = null,
        Guid? assignedToFacultyId = null,
        string? assignedAuditorName = null,
        string? notes = null,
        Guid? tenantId = null)
        : base(id)
    {
        AuditNumber = Check.NotNullOrWhiteSpace(auditNumber, nameof(auditNumber), AuditConsts.MaxAuditNumberLength);
        WarehouseId = warehouseId;
        CountType = countType;
        ScheduledDate = scheduledDate;
        IsBlindCount = isBlindCount;
        ZoneId = zoneId;
        AssignedToFacultyId = assignedToFacultyId;
        AssignedAuditorName = assignedAuditorName;
        Notes = notes;
        TenantId = tenantId;
        Status = CycleCountStatus.Draft;
    }
}

