using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Faculty;

public class GetFacultyMemberListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public Guid? AssignedWarehouseId { get; set; }

    public string? Department { get; set; }

    public string? Shift { get; set; }

    public bool? IsActive { get; set; }
}

