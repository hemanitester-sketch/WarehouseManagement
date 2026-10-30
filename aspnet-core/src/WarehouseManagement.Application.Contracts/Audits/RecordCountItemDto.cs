using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Audits;

public class RecordCountItemDto
{
    public Guid ItemId { get; set; }

    [Required]
    public decimal CountedQuantity { get; set; }

    public DiscrepancyReason DiscrepancyReason { get; set; }

    [StringLength(AuditConsts.MaxReasonNotesLength)]
    public string? ReasonNotes { get; set; }
}

public class SubmitCountDto
{
    public string? Notes { get; set; }
}

public class ApproveReconcileAuditDto
{
    public string? ApproverName { get; set; }

    public string? ApprovalNotes { get; set; }
}

