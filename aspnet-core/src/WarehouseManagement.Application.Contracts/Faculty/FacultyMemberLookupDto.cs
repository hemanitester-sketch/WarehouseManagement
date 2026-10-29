using System;

namespace WarehouseManagement.Faculty;

public class FacultyMemberLookupDto
{
    public Guid Id { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public string? Department { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public Guid? AssignedWarehouseId { get; set; }

    public string DisplayName { get; set; } = null!;
}

