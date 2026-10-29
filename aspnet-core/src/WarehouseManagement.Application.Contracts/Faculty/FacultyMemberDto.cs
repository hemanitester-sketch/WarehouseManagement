using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Faculty;

public class FacultyMemberDto : FullAuditedEntityDto<Guid>
{
    public string EmployeeCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public string? Department { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public Guid? AssignedWarehouseId { get; set; }

    public Guid? WarehouseId
    {
        get => AssignedWarehouseId;
        set => AssignedWarehouseId = value;
    }

    public string? AssignedWarehouseName { get; set; }

    public string? WarehouseName
    {
        get => AssignedWarehouseName;
        set => AssignedWarehouseName = value;
    }

    public string? AssignedWarehouseCode { get; set; }

    public string? WarehouseCode
    {
        get => AssignedWarehouseCode;
        set => AssignedWarehouseCode = value;
    }

    public string? Shift { get; set; }

    public DateTime? JoiningDate { get; set; }

    public string? Qualifications { get; set; }

    public bool IsActive { get; set; }

    public string Status => IsActive ? "Active" : "Inactive";
}
