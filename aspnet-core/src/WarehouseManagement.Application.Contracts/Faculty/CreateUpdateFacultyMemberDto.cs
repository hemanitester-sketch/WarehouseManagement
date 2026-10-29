using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Faculty;

public class CreateUpdateFacultyMemberDto
{
    [Required]
    [StringLength(FacultyConsts.MaxEmployeeCodeLength)]
    public string EmployeeCode { get; set; } = null!;

    [Required]
    [StringLength(FacultyConsts.MaxFullNameLength)]
    public string FullName { get; set; } = null!;

    [Required]
    [StringLength(FacultyConsts.MaxDesignationLength)]
    public string Designation { get; set; } = null!;

    [StringLength(FacultyConsts.MaxDepartmentLength)]
    public string? Department { get; set; }

    [EmailAddress]
    [StringLength(FacultyConsts.MaxEmailLength)]
    public string? Email { get; set; }

    [StringLength(FacultyConsts.MaxPhoneLength)]
    public string? PhoneNumber { get; set; }

    [StringLength(FacultyConsts.MaxEmergencyContactLength)]
    public string? EmergencyContactName { get; set; }

    [StringLength(FacultyConsts.MaxPhoneLength)]
    public string? EmergencyContactPhone { get; set; }

    public Guid? AssignedWarehouseId { get; set; }

    public Guid? WarehouseId
    {
        get => AssignedWarehouseId;
        set => AssignedWarehouseId = value;
    }

    [StringLength(FacultyConsts.MaxShiftLength)]
    public string? Shift { get; set; } = "General (Day)";

    public DateTime? JoiningDate { get; set; }

    [StringLength(FacultyConsts.MaxQualificationsLength)]
    public string? Qualifications { get; set; }

    public string? Status { get; set; }

    public bool IsActive { get; set; } = true;
}
