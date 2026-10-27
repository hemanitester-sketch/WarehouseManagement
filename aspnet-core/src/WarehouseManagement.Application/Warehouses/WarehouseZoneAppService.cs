using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Permissions;

namespace WarehouseManagement.Warehouses;

public class WarehouseZoneAppService :
    CrudAppService<
        WarehouseZone,
        WarehouseZoneDto,
        Guid,
        GetWarehouseZoneListDto,
        CreateUpdateWarehouseZoneDto,
        CreateUpdateWarehouseZoneDto>,
    IWarehouseZoneAppService
{
    public WarehouseZoneAppService(IRepository<WarehouseZone, Guid> repository)
        : base(repository)
    {
        GetPolicyName = null;
        GetListPolicyName = null;
        CreatePolicyName = null;
        UpdatePolicyName = null;
        DeletePolicyName = null;
    }

    /// <summary>
    /// Creates a new warehouse zone after validating uniqueness via LINQ query.
    /// Triggered by POST /api/app/warehouse-zone.
    /// </summary>
    public override async Task<WarehouseZoneDto> CreateAsync(CreateUpdateWarehouseZoneDto input)
    {
        // 1. Get queryable source for LINQ operations
        var queryable = await Repository.GetQueryableAsync();

        // 2. Validate uniqueness: Check if a zone with the same code already exists in this warehouse
        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        bool isDuplicate = queryable.Any(x => x.WarehouseId == input.WarehouseId && x.Code == normalizedCode);

        if (isDuplicate)
        {
            throw new UserFriendlyException($"A zone with code '{input.Code}' already exists in this warehouse.");
        }

        // 3. Instantiate new WarehouseZone entity
        var zone = new WarehouseZone(
            GuidGenerator.Create(),
            input.WarehouseId,
            input.Code,
            input.Name,
            input.StorageType,
            CurrentTenant.Id,
            input.TemperatureMinCelsius,
            input.TemperatureMaxCelsius,
            input.IsActive
        );

        // 4. Insert entity into database
        await Repository.InsertAsync(zone, autoSave: true);

        // 5. Map and return the output DTO
        return MapToGetOutputDto(zone);
    }

    /// <summary>
    /// Updates an existing warehouse zone after validating uniqueness.
    /// Triggered by PUT /api/app/warehouse-zone/{id}.
    /// </summary>
    public override async Task<WarehouseZoneDto> UpdateAsync(Guid id, CreateUpdateWarehouseZoneDto input)
    {
        // 1. Retrieve the existing zone by Id
        var zone = await Repository.GetAsync(id);

        // 2. Validate uniqueness against other zones in the same warehouse
        var queryable = await Repository.GetQueryableAsync();
        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        bool isDuplicate = queryable.Any(x => x.Id != id && x.WarehouseId == input.WarehouseId && x.Code == normalizedCode);

        if (isDuplicate)
        {
            throw new UserFriendlyException($"A zone with code '{input.Code}' already exists in this warehouse.");
        }

        // 3. Update entity properties
        zone.WarehouseId = input.WarehouseId;
        zone.SetCode(input.Code);
        zone.SetName(input.Name);
        zone.StorageType = input.StorageType;
        zone.TemperatureMinCelsius = input.TemperatureMinCelsius;
        zone.TemperatureMaxCelsius = input.TemperatureMaxCelsius;
        zone.IsActive = input.IsActive;

        // 4. Save updates to database
        await Repository.UpdateAsync(zone, autoSave: true);

        // 5. Map and return updated DTO
        return MapToGetOutputDto(zone);
    }

    protected override async Task<IQueryable<WarehouseZone>> CreateFilteredQueryAsync(GetWarehouseZoneListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!))
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId!.Value)
            .WhereIf(input.StorageType.HasValue, x => x.StorageType == input.StorageType!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);
    }

    protected override Task<WarehouseZone> MapToEntityAsync(CreateUpdateWarehouseZoneDto createInput)
    {
        var entity = new WarehouseZone(
            GuidGenerator.Create(),
            createInput.WarehouseId,
            createInput.Code,
            createInput.Name,
            createInput.StorageType,
            CurrentTenant.Id,
            createInput.TemperatureMinCelsius,
            createInput.TemperatureMaxCelsius,
            createInput.IsActive
        );

        return Task.FromResult(entity);
    }

    protected override Task MapToEntityAsync(CreateUpdateWarehouseZoneDto updateInput, WarehouseZone entity)
    {
        entity.WarehouseId = updateInput.WarehouseId;
        entity.SetCode(updateInput.Code);
        entity.SetName(updateInput.Name);
        entity.StorageType = updateInput.StorageType;
        entity.TemperatureMinCelsius = updateInput.TemperatureMinCelsius;
        entity.TemperatureMaxCelsius = updateInput.TemperatureMaxCelsius;
        entity.IsActive = updateInput.IsActive;

        return Task.CompletedTask;
    }

    protected override WarehouseZoneDto MapToGetOutputDto(WarehouseZone entity)
    {
        return new WarehouseZoneDto
        {
            Id = entity.Id,
            WarehouseId = entity.WarehouseId,
            Code = entity.Code,
            Name = entity.Name,
            StorageType = entity.StorageType,
            TemperatureMinCelsius = entity.TemperatureMinCelsius,
            TemperatureMaxCelsius = entity.TemperatureMaxCelsius,
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    protected override Task<List<WarehouseZoneDto>> MapToGetListOutputDtosAsync(List<WarehouseZone> entities)
    {
        var dtos = entities.Select(MapToGetOutputDto).ToList();
        return Task.FromResult(dtos);
    }
}
