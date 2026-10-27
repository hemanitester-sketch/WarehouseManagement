using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Permissions;

namespace WarehouseManagement.Warehouses;

public class WarehouseLocationAppService :
    CrudAppService<
        WarehouseLocation,
        WarehouseLocationDto,
        Guid,
        GetWarehouseLocationListDto,
        CreateUpdateWarehouseLocationDto,
        CreateUpdateWarehouseLocationDto>,
    IWarehouseLocationAppService
{
    public WarehouseLocationAppService(IRepository<WarehouseLocation, Guid> repository)
        : base(repository)
    {
        GetPolicyName = null;
        GetListPolicyName = null;
        CreatePolicyName = null;
        UpdatePolicyName = null;
        DeletePolicyName = null;
    }

    public async Task<WarehouseLocationDto?> FindByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return null;
        }

        var normalizedBarcode = barcode.Trim().ToUpperInvariant();
        var entity = await Repository.FirstOrDefaultAsync(x => x.Barcode == normalizedBarcode);

        return entity == null ? null : MapToGetOutputDto(entity);
    }

    protected override async Task<IQueryable<WarehouseLocation>> CreateFilteredQueryAsync(GetWarehouseLocationListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.Barcode.Contains(input.Filter!) ||
                     x.Aisle.Contains(input.Filter!) ||
                     x.Rack.Contains(input.Filter!) ||
                     x.Shelf.Contains(input.Filter!) ||
                     x.Bin.Contains(input.Filter!))
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId!.Value)
            .WhereIf(input.ZoneId.HasValue, x => x.ZoneId == input.ZoneId!.Value)
            .WhereIf(input.LocationType.HasValue, x => x.LocationType == input.LocationType!.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status!.Value)
            .WhereIf(input.IsPickFace.HasValue, x => x.IsPickFace == input.IsPickFace!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);
    }

    protected override Task<WarehouseLocation> MapToEntityAsync(CreateUpdateWarehouseLocationDto createInput)
    {
        var entity = new WarehouseLocation(
            GuidGenerator.Create(),
            createInput.WarehouseId,
            createInput.ZoneId,
            createInput.Barcode,
            createInput.Aisle,
            createInput.Rack,
            createInput.Shelf,
            createInput.Bin,
            createInput.LocationType,
            CurrentTenant.Id,
            createInput.Status,
            createInput.MaxWeightKg,
            createInput.MaxVolumeCm3,
            createInput.LengthCm,
            createInput.WidthCm,
            createInput.HeightCm,
            createInput.IsPickFace,
            createInput.IsActive
        );

        return Task.FromResult(entity);
    }

    protected override Task MapToEntityAsync(CreateUpdateWarehouseLocationDto updateInput, WarehouseLocation entity)
    {
        entity.WarehouseId = updateInput.WarehouseId;
        entity.ZoneId = updateInput.ZoneId;
        entity.SetBarcode(updateInput.Barcode);
        entity.SetCoordinates(updateInput.Aisle, updateInput.Rack, updateInput.Shelf, updateInput.Bin);
        entity.LocationType = updateInput.LocationType;
        entity.Status = updateInput.Status;
        entity.MaxWeightKg = updateInput.MaxWeightKg;
        entity.MaxVolumeCm3 = updateInput.MaxVolumeCm3;
        entity.LengthCm = updateInput.LengthCm;
        entity.WidthCm = updateInput.WidthCm;
        entity.HeightCm = updateInput.HeightCm;
        entity.IsPickFace = updateInput.IsPickFace;
        entity.IsActive = updateInput.IsActive;

        return Task.CompletedTask;
    }

    protected override WarehouseLocationDto MapToGetOutputDto(WarehouseLocation entity)
    {
        return new WarehouseLocationDto
        {
            Id = entity.Id,
            WarehouseId = entity.WarehouseId,
            ZoneId = entity.ZoneId,
            Barcode = entity.Barcode,
            Aisle = entity.Aisle,
            Rack = entity.Rack,
            Shelf = entity.Shelf,
            Bin = entity.Bin,
            LocationType = entity.LocationType,
            Status = entity.Status,
            MaxWeightKg = entity.MaxWeightKg,
            MaxVolumeCm3 = entity.MaxVolumeCm3,
            LengthCm = entity.LengthCm,
            WidthCm = entity.WidthCm,
            HeightCm = entity.HeightCm,
            IsPickFace = entity.IsPickFace,
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    protected override Task<List<WarehouseLocationDto>> MapToGetListOutputDtosAsync(List<WarehouseLocation> entities)
    {
        var dtos = entities.Select(MapToGetOutputDto).ToList();
        return Task.FromResult(dtos);
    }
}
