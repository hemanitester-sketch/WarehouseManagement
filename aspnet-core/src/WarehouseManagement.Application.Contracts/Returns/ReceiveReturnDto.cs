using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Returns;

public class ReceiveReturnItemDto
{
    [Required]
    public Guid ItemId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ReceivedQuantity { get; set; }
}

public class ReceiveReturnDto
{
    public DateTime? ReceivedDate { get; set; }

    [StringLength(RmaConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    public List<ReceiveReturnItemDto> Items { get; set; } = new();
}

