using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Faculty;

public class FacultyMember : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string EmployeeCode { get; private set; } = null!;

    public string FullName { get; private set; } = null!;

    public string Designation { get; set; } = null!;

    public string? Department { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public Guid? AssignedWarehouseId { get; set; }

    public string? Shift { get; set; }

    public DateTime? JoiningDate { get; set; }

    public string? Qualifications { get; set; }

    public bool IsActive { get; set; }

    public FacultyMember()
    {
    }

    public FacultyMember(
        Guid id,
        string employeeCode,
        string fullName,
        string designation,
        string? department = null,
        string? email = null,
        string? phoneNumber = null,
        Guid? assignedWarehouseId = null,
        string? shift = "General (Day)",
        DateTime? joiningDate = null,
        string? qualifications = null,
        string? emergencyContactName = null,
        string? emergencyContactPhone = null,
        bool isActive = true,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        SetEmployeeCode(employeeCode);
        SetFullName(fullName);
        Designation = designation.Trim();
        Department = department?.Trim();
        Email = email?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        AssignedWarehouseId = assignedWarehouseId;
        Shift = shift?.Trim() ?? "General (Day)";
        JoiningDate = joiningDate;
        Qualifications = qualifications?.Trim();
        EmergencyContactName = emergencyContactName?.Trim();
        EmergencyContactPhone = emergencyContactPhone?.Trim();
        IsActive = isActive;
    }

    public FacultyMember SetEmployeeCode(string employeeCode)
    {
        Check.NotNullOrWhiteSpace(employeeCode, nameof(employeeCode), FacultyConsts.MaxEmployeeCodeLength);
        EmployeeCode = employeeCode.Trim().ToUpperInvariant();
        return this;
    }

    public FacultyMember SetFullName(string fullName)
    {
        Check.NotNullOrWhiteSpace(fullName, nameof(fullName), FacultyConsts.MaxFullNameLength);
        FullName = fullName.Trim();
        return this;
    }
}

