using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Returns;

public class InspectItemResolutionDto
{
    [Required]
    public Guid ItemId { get; set; }

    [Required]
    public ItemConditionGrade ConditionGrade { get; set; }

    [Required]
    public ItemResolutionAction ResolutionAction { get; set; }

    [Range(0, double.MaxValue)]
    public decimal RestockedQuantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ScrappedQuantity { get; set; }

    public Guid? DestinationLocationId { get; set; }

    [StringLength(RmaConsts.MaxInspectionNotesLength)]
    public string? InspectionNotes { get; set; }
}

public class InspectReturnOrderDto
{
    [StringLength(RmaConsts.MaxCustomerNameLength)]
    public string? InspectorName { get; set; }

    public List<InspectItemResolutionDto> Items { get; set; } = new();
}
