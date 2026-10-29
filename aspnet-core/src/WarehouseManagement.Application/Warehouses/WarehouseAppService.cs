using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Faculty;
using WarehouseManagement.Permissions;

namespace WarehouseManagement.Warehouses;

public class WarehouseAppService :
    CrudAppService<
        Warehouse,
        WarehouseDto,
        Guid,
        GetWarehouseListDto,
        CreateUpdateWarehouseDto,
        CreateUpdateWarehouseDto>,
    IWarehouseAppService
{
    private readonly IRepository<FacultyMember, Guid> _facultyRepository;

    public WarehouseAppService(
        IRepository<Warehouse, Guid> repository,
        IRepository<FacultyMember, Guid> facultyRepository)
        : base(repository)
    {
        _facultyRepository = facultyRepository;
        GetPolicyName = null;
        GetListPolicyName = null;
        CreatePolicyName = null;
        UpdatePolicyName = null;
        DeletePolicyName = null;
    }

    /// <summary>
    /// Creates a new warehouse after validating required fields, code and name uniqueness via LINQ query.
    /// Triggered by POST /api/app/warehouse.
    /// </summary>
    public override async Task<WarehouseDto> CreateAsync(CreateUpdateWarehouseDto input)
    {
        // 1. Validate required fields
        ValidateWarehouseInput(input);

        // 2. Get queryable source for LINQ operations
        var queryable = await Repository.GetQueryableAsync();

        // 3. Validate uniqueness: Check if warehouse Code already exists
        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        if (queryable.Any(x => x.Code == normalizedCode))
        {
            throw new UserFriendlyException($"A warehouse with code '{input.Code}' already exists. Please choose a different warehouse code.");
        }

        // 4. Validate uniqueness: Check if warehouse Name already exists
        var normalizedName = input.Name.Trim().ToLower();
        if (queryable.Any(x => x.Name.ToLower() == normalizedName))
        {
            throw new UserFriendlyException($"A warehouse with name '{input.Name}' already exists. Please choose a different warehouse name.");
        }

        // 4. Instantiate new Warehouse entity
        var warehouse = new Warehouse(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Type,
            CurrentTenant.Id,
            input.Address,
            input.City,
            input.State,
            input.PostalCode,
            input.Country,
            input.ContactPerson,
            input.ContactPhone,
            input.ContactEmail,
            input.TotalAreaSqFt,
            input.IsActive,
            input.ManagerId
        );

        // 5. Insert entity into database
        await Repository.InsertAsync(warehouse, autoSave: true);

        // 6. Map and return the output DTO
        return await GetAsync(warehouse.Id);
    }

    /// <summary>
    /// Updates an existing warehouse after validating code and name uniqueness.
    /// Triggered by PUT /api/app/warehouse/{id}.
    /// </summary>
    public override async Task<WarehouseDto> UpdateAsync(Guid id, CreateUpdateWarehouseDto input)
    {
        // 1. Validate required fields
        ValidateWarehouseInput(input);

        // 2. Retrieve existing warehouse by Id
        var warehouse = await Repository.GetAsync(id);

        // 3. Validate uniqueness against other warehouses for Code
        var queryable = await Repository.GetQueryableAsync();
        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        if (queryable.Any(x => x.Id != id && x.Code == normalizedCode))
        {
            throw new UserFriendlyException($"A warehouse with code '{input.Code}' already exists. Please choose a different warehouse code.");
        }

        // 4. Validate uniqueness against other warehouses for Name
        var normalizedName = input.Name.Trim().ToLower();
        if (queryable.Any(x => x.Id != id && x.Name.ToLower() == normalizedName))
        {
            throw new UserFriendlyException($"A warehouse with name '{input.Name}' already exists. Please choose a different warehouse name.");
        }

        // 5. Update entity properties
        warehouse.SetCode(input.Code);
        warehouse.SetName(input.Name);
        warehouse.Type = input.Type;
        warehouse.Address = input.Address;
        warehouse.City = input.City;
        warehouse.State = input.State;
        warehouse.PostalCode = input.PostalCode;
        warehouse.Country = input.Country;
        warehouse.ContactPerson = input.ContactPerson;
        warehouse.ContactPhone = input.ContactPhone;
        warehouse.ContactEmail = input.ContactEmail;
        warehouse.TotalAreaSqFt = input.TotalAreaSqFt;
        warehouse.IsActive = input.IsActive;
        warehouse.ManagerId = input.ManagerId;

        // 6. Save updates to database
        await Repository.UpdateAsync(warehouse, autoSave: true);

        // 7. Map and return updated DTO
        return await GetAsync(warehouse.Id);
    }

    private static void ValidateWarehouseInput(CreateUpdateWarehouseDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
            throw new UserFriendlyException("Warehouse code is required.");

        if (string.IsNullOrWhiteSpace(input.Name))
            throw new UserFriendlyException("Warehouse name is required.");

        if (string.IsNullOrWhiteSpace(input.Address))
            throw new UserFriendlyException("Street address is required.");

        if (string.IsNullOrWhiteSpace(input.City))
            throw new UserFriendlyException("City is required.");

        if (string.IsNullOrWhiteSpace(input.State))
            throw new UserFriendlyException("State is required.");

        if (string.IsNullOrWhiteSpace(input.PostalCode))
            throw new UserFriendlyException("Postal code is required.");

        if (string.IsNullOrWhiteSpace(input.ContactPerson))
            throw new UserFriendlyException("Facility manager name is required.");

        if (string.IsNullOrWhiteSpace(input.ContactPhone))
            throw new UserFriendlyException("Contact phone number is required.");

        if (input.TotalAreaSqFt <= 0)
            throw new UserFriendlyException("Total floor area must be greater than 0 sq.ft.");
    }

    protected override async Task<IQueryable<Warehouse>> CreateFilteredQueryAsync(GetWarehouseListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!) || (x.City != null && x.City.Contains(input.Filter!)))
            .WhereIf(input.Type.HasValue, x => x.Type == input.Type!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);
    }

    protected override Task<Warehouse> MapToEntityAsync(CreateUpdateWarehouseDto createInput)
    {
        var entity = new Warehouse(
            GuidGenerator.Create(),
            createInput.Code,
            createInput.Name,
            createInput.Type,
            CurrentTenant.Id,
            createInput.Address,
            createInput.City,
            createInput.State,
            createInput.PostalCode,
            createInput.Country,
            createInput.ContactPerson,
            createInput.ContactPhone,
            createInput.ContactEmail,
            createInput.TotalAreaSqFt,
            createInput.IsActive,
            createInput.ManagerId
        );

        return Task.FromResult(entity);
    }

    protected override Task MapToEntityAsync(CreateUpdateWarehouseDto updateInput, Warehouse entity)
    {
        entity.SetCode(updateInput.Code);
        entity.SetName(updateInput.Name);
        entity.Type = updateInput.Type;
        entity.Address = updateInput.Address;
        entity.City = updateInput.City;
        entity.State = updateInput.State;
        entity.PostalCode = updateInput.PostalCode;
        entity.Country = updateInput.Country;
        entity.ContactPerson = updateInput.ContactPerson;
        entity.ContactPhone = updateInput.ContactPhone;
        entity.ContactEmail = updateInput.ContactEmail;
        entity.TotalAreaSqFt = updateInput.TotalAreaSqFt;
        entity.IsActive = updateInput.IsActive;
        entity.ManagerId = updateInput.ManagerId;

        return Task.CompletedTask;
    }

    protected override WarehouseDto MapToGetOutputDto(Warehouse entity)
    {
        return new WarehouseDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Type = entity.Type,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            ContactPerson = entity.ContactPerson,
            ContactPhone = entity.ContactPhone,
            ContactEmail = entity.ContactEmail,
            ManagerId = entity.ManagerId,
            TotalAreaSqFt = entity.TotalAreaSqFt,
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    public override async Task<WarehouseDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        var dto = MapToGetOutputDto(entity);

        if (entity.ManagerId.HasValue)
        {
            var manager = await _facultyRepository.FindAsync(entity.ManagerId.Value);
            if (manager != null)
            {
                dto.ManagerName = manager.FullName;
                dto.ManagerDesignation = manager.Designation;
            }
        }

        return dto;
    }

    protected override async Task<List<WarehouseDto>> MapToGetListOutputDtosAsync(List<Warehouse> entities)
    {
        if (entities.Count == 0)
        {
            return new List<WarehouseDto>();
        }

        var managerIds = entities
            .Where(w => w.ManagerId.HasValue)
            .Select(w => w.ManagerId!.Value)
            .Distinct()
            .ToList();

        var managers = managerIds.Count > 0
            ? await _facultyRepository.GetListAsync(m => managerIds.Contains(m.Id))
            : new List<FacultyMember>();

        var managerMap = managers.ToDictionary(m => m.Id);

        var dtos = entities.Select(w =>
        {
            var dto = MapToGetOutputDto(w);
            if (w.ManagerId.HasValue && managerMap.TryGetValue(w.ManagerId.Value, out var mgr))
            {
                dto.ManagerName = mgr.FullName;
                dto.ManagerDesignation = mgr.Designation;
            }
            return dto;
        }).ToList();

        return dtos;
    }
}
