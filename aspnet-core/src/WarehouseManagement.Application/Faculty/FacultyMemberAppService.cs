using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Faculty;

public class FacultyMemberAppService :
    CrudAppService<
        FacultyMember,
        FacultyMemberDto,
        Guid,
        GetFacultyMemberListDto,
        CreateUpdateFacultyMemberDto,
        CreateUpdateFacultyMemberDto>,
    IFacultyMemberAppService
{
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;

    public FacultyMemberAppService(
        IRepository<FacultyMember, Guid> repository,
        IRepository<Warehouse, Guid> warehouseRepository)
        : base(repository)
    {
        _warehouseRepository = warehouseRepository;
        GetPolicyName = null;
        GetListPolicyName = null;
        CreatePolicyName = null;
        UpdatePolicyName = null;
        DeletePolicyName = null;
    }

    public override async Task<FacultyMemberDto> CreateAsync(CreateUpdateFacultyMemberDto input)
    {
        ValidateFacultyInput(input);

        var normalizedCode = input.EmployeeCode.Trim().ToUpperInvariant();
        var codeExists = await Repository.AnyAsync(f => f.EmployeeCode == normalizedCode);
        if (codeExists)
        {
            throw new UserFriendlyException($"A faculty/staff member with Employee Code '{normalizedCode}' already exists. Please choose a unique code.");
        }

        if (input.AssignedWarehouseId.HasValue)
        {
            var warehouse = await _warehouseRepository.FindAsync(input.AssignedWarehouseId.Value);
            if (warehouse == null)
            {
                throw new UserFriendlyException("The assigned warehouse facility does not exist.");
            }
        }

        var entity = await MapToEntityAsync(input);
        TryToSetTenantId(entity);
        await Repository.InsertAsync(entity, autoSave: true);

        return await GetAsync(entity.Id);
    }

    public override async Task<FacultyMemberDto> UpdateAsync(Guid id, CreateUpdateFacultyMemberDto input)
    {
        ValidateFacultyInput(input);

        var entity = await Repository.GetAsync(id);

        var normalizedCode = input.EmployeeCode.Trim().ToUpperInvariant();
        var codeExists = await Repository.AnyAsync(f => f.Id != id && f.EmployeeCode == normalizedCode);
        if (codeExists)
        {
            throw new UserFriendlyException($"Another faculty/staff member with Employee Code '{normalizedCode}' already exists. Please choose a unique code.");
        }

        if (input.AssignedWarehouseId.HasValue)
        {
            var warehouse = await _warehouseRepository.FindAsync(input.AssignedWarehouseId.Value);
            if (warehouse == null)
            {
                throw new UserFriendlyException("The assigned warehouse facility does not exist.");
            }
        }

        entity.SetEmployeeCode(input.EmployeeCode);
        entity.SetFullName(input.FullName);
        entity.Designation = input.Designation.Trim();
        entity.Department = input.Department?.Trim();
        entity.Email = input.Email?.Trim();
        entity.PhoneNumber = input.PhoneNumber?.Trim();
        entity.EmergencyContactName = input.EmergencyContactName?.Trim();
        entity.EmergencyContactPhone = input.EmergencyContactPhone?.Trim();
        entity.AssignedWarehouseId = input.AssignedWarehouseId;
        entity.Shift = input.Shift?.Trim() ?? "General (Day)";
        entity.JoiningDate = input.JoiningDate;
        entity.Qualifications = input.Qualifications?.Trim();
        entity.IsActive = input.IsActive;

        await Repository.UpdateAsync(entity, autoSave: true);

        return await GetAsync(entity.Id);
    }

    private static void ValidateFacultyInput(CreateUpdateFacultyMemberDto input)
    {
        if (string.IsNullOrWhiteSpace(input.EmployeeCode))
        {
            throw new UserFriendlyException("Employee / Staff code is required.");
        }

        if (string.IsNullOrWhiteSpace(input.FullName))
        {
            throw new UserFriendlyException("Full Name is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Designation))
        {
            throw new UserFriendlyException("Designation / Role is required.");
        }
    }

    public async Task<FacultyMemberDto?> FindByCodeAsync(string employeeCode)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            return null;
        }

        var normalized = employeeCode.Trim().ToUpperInvariant();
        var entity = await Repository.FirstOrDefaultAsync(f => f.EmployeeCode == normalized);
        return entity == null ? null : await GetAsync(entity.Id);
    }

    public async Task<ListResultDto<FacultyMemberLookupDto>> GetLookupAsync(Guid? warehouseId = null)
    {
        var members = await Repository.GetListAsync(f => f.IsActive);

        if (warehouseId.HasValue)
        {
            members = members.Where(f => !f.AssignedWarehouseId.HasValue || f.AssignedWarehouseId == warehouseId.Value).ToList();
        }

        var lookups = members
            .OrderBy(f => f.FullName)
            .Select(f => new FacultyMemberLookupDto
            {
                Id = f.Id,
                EmployeeCode = f.EmployeeCode,
                FullName = f.FullName,
                Designation = f.Designation,
                Department = f.Department,
                PhoneNumber = f.PhoneNumber,
                Email = f.Email,
                AssignedWarehouseId = f.AssignedWarehouseId,
                DisplayName = $"{f.FullName} ({f.Designation}) - {f.EmployeeCode}"
            })
            .ToList();

        return new ListResultDto<FacultyMemberLookupDto>(lookups);
    }

    protected override async Task<IQueryable<FacultyMember>> CreateFilteredQueryAsync(GetFacultyMemberListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                f => f.EmployeeCode.Contains(input.Filter!) ||
                     f.FullName.Contains(input.Filter!) ||
                     (f.Designation != null && f.Designation.Contains(input.Filter!)) ||
                     (f.Department != null && f.Department.Contains(input.Filter!)) ||
                     (f.PhoneNumber != null && f.PhoneNumber.Contains(input.Filter!)) ||
                     (f.Email != null && f.Email.Contains(input.Filter!)))
            .WhereIf(input.AssignedWarehouseId.HasValue, f => f.AssignedWarehouseId == input.AssignedWarehouseId!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Department), f => f.Department == input.Department!)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Shift), f => f.Shift == input.Shift!)
            .WhereIf(input.IsActive.HasValue, f => f.IsActive == input.IsActive!.Value);
    }

    public override async Task<FacultyMemberDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        var dto = MapToGetOutputDto(entity);

        if (entity.AssignedWarehouseId.HasValue)
        {
            var wh = await _warehouseRepository.FindAsync(entity.AssignedWarehouseId.Value);
            if (wh != null)
            {
                dto.AssignedWarehouseName = wh.Name;
                dto.AssignedWarehouseCode = wh.Code;
            }
        }

        return dto;
    }

    protected override async Task<List<FacultyMemberDto>> MapToGetListOutputDtosAsync(List<FacultyMember> entities)
    {
        if (entities.Count == 0)
        {
            return new List<FacultyMemberDto>();
        }

        var warehouseIds = entities
            .Where(f => f.AssignedWarehouseId.HasValue)
            .Select(f => f.AssignedWarehouseId!.Value)
            .Distinct()
            .ToList();

        var warehouses = warehouseIds.Count > 0
            ? await _warehouseRepository.GetListAsync(w => warehouseIds.Contains(w.Id))
            : new List<Warehouse>();

        var whMap = warehouses.ToDictionary(w => w.Id);

        var dtos = entities.Select(f =>
        {
            var dto = MapToGetOutputDto(f);
            if (f.AssignedWarehouseId.HasValue && whMap.TryGetValue(f.AssignedWarehouseId.Value, out var wh))
            {
                dto.AssignedWarehouseName = wh.Name;
                dto.AssignedWarehouseCode = wh.Code;
            }
            return dto;
        }).ToList();

        return dtos;
    }

    protected override Task<FacultyMember> MapToEntityAsync(CreateUpdateFacultyMemberDto createInput)
    {
        var entity = new FacultyMember(
            GuidGenerator.Create(),
            createInput.EmployeeCode,
            createInput.FullName,
            createInput.Designation,
            createInput.Department,
            createInput.Email,
            createInput.PhoneNumber,
            createInput.AssignedWarehouseId,
            createInput.Shift,
            createInput.JoiningDate,
            createInput.Qualifications,
            createInput.EmergencyContactName,
            createInput.EmergencyContactPhone,
            createInput.IsActive,
            CurrentTenant.Id
        );

        return Task.FromResult(entity);
    }

    protected override FacultyMemberDto MapToGetOutputDto(FacultyMember entity)
    {
        return new FacultyMemberDto
        {
            Id = entity.Id,
            EmployeeCode = entity.EmployeeCode,
            FullName = entity.FullName,
            Designation = entity.Designation,
            Department = entity.Department,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            EmergencyContactName = entity.EmergencyContactName,
            EmergencyContactPhone = entity.EmergencyContactPhone,
            AssignedWarehouseId = entity.AssignedWarehouseId,
            Shift = entity.Shift,
            JoiningDate = entity.JoiningDate,
            Qualifications = entity.Qualifications,
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime
        };
    }
}

