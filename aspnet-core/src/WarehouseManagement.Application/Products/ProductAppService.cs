using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Permissions;

namespace WarehouseManagement.Products;

public class ProductAppService :
    CrudAppService<
        Product,
        ProductDto,
        Guid,
        GetProductListDto,
        CreateUpdateProductDto,
        CreateUpdateProductDto>,
    IProductAppService
{
    public ProductAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {
        GetPolicyName = null;
        GetListPolicyName = null;
        CreatePolicyName = null;
        UpdatePolicyName = null;
        DeletePolicyName = null;
    }

    public override async Task<ProductDto> CreateAsync(CreateUpdateProductDto input)
    {
        ValidateProductInput(input);

        var normalizedSku = input.Sku.Trim().ToUpperInvariant();
        var skuExists = await Repository.AnyAsync(p => p.Sku == normalizedSku);
        if (skuExists)
        {
            throw new UserFriendlyException($"A product with SKU '{normalizedSku}' already exists. Please specify a unique SKU.");
        }

        if (!string.IsNullOrWhiteSpace(input.Barcode))
        {
            var normalizedBarcode = input.Barcode.Trim();
            var barcodeExists = await Repository.AnyAsync(p => p.Barcode == normalizedBarcode);
            if (barcodeExists)
            {
                throw new UserFriendlyException($"A product with Barcode '{normalizedBarcode}' already exists. Please specify a unique barcode.");
            }
        }

        var entity = await MapToEntityAsync(input);
        TryToSetTenantId(entity);
        await Repository.InsertAsync(entity, autoSave: true);

        return MapToGetOutputDto(entity);
    }

    public override async Task<ProductDto> UpdateAsync(Guid id, CreateUpdateProductDto input)
    {
        ValidateProductInput(input);

        var entity = await Repository.GetAsync(id);

        var normalizedSku = input.Sku.Trim().ToUpperInvariant();
        var skuExists = await Repository.AnyAsync(p => p.Id != id && p.Sku == normalizedSku);
        if (skuExists)
        {
            throw new UserFriendlyException($"Another product with SKU '{normalizedSku}' already exists. Please specify a unique SKU.");
        }

        if (!string.IsNullOrWhiteSpace(input.Barcode))
        {
            var normalizedBarcode = input.Barcode.Trim();
            var barcodeExists = await Repository.AnyAsync(p => p.Id != id && p.Barcode == normalizedBarcode);
            if (barcodeExists)
            {
                throw new UserFriendlyException($"Another product with Barcode '{normalizedBarcode}' already exists. Please specify a unique barcode.");
            }
        }

        await MapToEntityAsync(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);

        return MapToGetOutputDto(entity);
    }

    private static void ValidateProductInput(CreateUpdateProductDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Sku))
        {
            throw new UserFriendlyException("Product SKU code is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new UserFriendlyException("Product name is required.");
        }

        if (input.UnitPrice < 0)
        {
            throw new UserFriendlyException("Unit price cannot be negative.");
        }

        if (input.ReorderLevel < 0)
        {
            throw new UserFriendlyException("Reorder level cannot be negative.");
        }

        if (input.TargetStockLevel < 0)
        {
            throw new UserFriendlyException("Target stock level cannot be negative.");
        }

        if (input.TemperatureMinCelsius.HasValue && input.TemperatureMaxCelsius.HasValue)
        {
            if (input.TemperatureMinCelsius.Value > input.TemperatureMaxCelsius.Value)
            {
                throw new UserFriendlyException("Minimum storage temperature cannot be greater than maximum temperature.");
            }
        }
    }

    public async Task<ProductDto?> FindBySkuAsync(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return null;
        }

        var normalizedSku = sku.Trim().ToUpperInvariant();
        var entity = await Repository.FirstOrDefaultAsync(x => x.Sku == normalizedSku);

        return entity == null ? null : MapToGetOutputDto(entity);
    }

    public async Task<ProductDto?> FindByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return null;
        }

        var normalizedBarcode = barcode.Trim();
        var entity = await Repository.FirstOrDefaultAsync(x => x.Barcode == normalizedBarcode);

        return entity == null ? null : MapToGetOutputDto(entity);
    }

    protected override async Task<IQueryable<Product>> CreateFilteredQueryAsync(GetProductListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.Sku.Contains(input.Filter!) ||
                     x.Name.Contains(input.Filter!) ||
                     (x.Barcode != null && x.Barcode.Contains(input.Filter!)) ||
                     (x.Brand != null && x.Brand.Contains(input.Filter!)))
            .WhereIf(input.Category.HasValue, x => x.Category == input.Category!.Value)
            .WhereIf(input.Uom.HasValue, x => x.Uom == input.Uom!.Value)
            .WhereIf(input.RequiredStorageType.HasValue, x => x.RequiredStorageType == input.RequiredStorageType!.Value)
            .WhereIf(input.IsLotTracked.HasValue, x => x.IsLotTracked == input.IsLotTracked!.Value)
            .WhereIf(input.IsExpiryTracked.HasValue, x => x.IsExpiryTracked == input.IsExpiryTracked!.Value)
            .WhereIf(input.IsSerialTracked.HasValue, x => x.IsSerialTracked == input.IsSerialTracked!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);
    }

    protected override Task<Product> MapToEntityAsync(CreateUpdateProductDto createInput)
    {
        var entity = new Product(
            GuidGenerator.Create(),
            createInput.Sku,
            createInput.Name,
            createInput.Category,
            createInput.Uom,
            CurrentTenant.Id,
            createInput.Barcode,
            createInput.Description,
            createInput.Brand,
            createInput.IsLotTracked,
            createInput.IsExpiryTracked,
            createInput.IsSerialTracked,
            createInput.WeightKg,
            createInput.VolumeCm3,
            createInput.LengthCm,
            createInput.WidthCm,
            createInput.HeightCm,
            createInput.RequiredStorageType,
            createInput.TemperatureMinCelsius,
            createInput.TemperatureMaxCelsius,
            createInput.ReorderLevel,
            createInput.TargetStockLevel,
            createInput.UnitPrice,
            createInput.IsActive
        );

        return Task.FromResult(entity);
    }

    protected override Task MapToEntityAsync(CreateUpdateProductDto updateInput, Product entity)
    {
        entity.SetSku(updateInput.Sku);
        entity.SetName(updateInput.Name);
        entity.Barcode = updateInput.Barcode?.Trim();
        entity.Description = updateInput.Description;
        entity.Brand = updateInput.Brand;
        entity.Category = updateInput.Category;
        entity.Uom = updateInput.Uom;
        entity.IsLotTracked = updateInput.IsLotTracked;
        entity.IsExpiryTracked = updateInput.IsExpiryTracked;
        entity.IsSerialTracked = updateInput.IsSerialTracked;
        entity.WeightKg = updateInput.WeightKg;
        entity.VolumeCm3 = updateInput.VolumeCm3;
        entity.LengthCm = updateInput.LengthCm;
        entity.WidthCm = updateInput.WidthCm;
        entity.HeightCm = updateInput.HeightCm;
        entity.RequiredStorageType = updateInput.RequiredStorageType;
        entity.TemperatureMinCelsius = updateInput.TemperatureMinCelsius;
        entity.TemperatureMaxCelsius = updateInput.TemperatureMaxCelsius;
        entity.ReorderLevel = updateInput.ReorderLevel;
        entity.TargetStockLevel = updateInput.TargetStockLevel;
        entity.UnitPrice = updateInput.UnitPrice;
        entity.IsActive = updateInput.IsActive;

        return Task.CompletedTask;
    }

    protected override ProductDto MapToGetOutputDto(Product entity)
    {
        return new ProductDto
        {
            Id = entity.Id,
            Sku = entity.Sku,
            Name = entity.Name,
            Barcode = entity.Barcode,
            Description = entity.Description,
            Brand = entity.Brand,
            Category = entity.Category,
            Uom = entity.Uom,
            IsLotTracked = entity.IsLotTracked,
            IsExpiryTracked = entity.IsExpiryTracked,
            IsSerialTracked = entity.IsSerialTracked,
            WeightKg = entity.WeightKg,
            VolumeCm3 = entity.VolumeCm3,
            LengthCm = entity.LengthCm,
            WidthCm = entity.WidthCm,
            HeightCm = entity.HeightCm,
            RequiredStorageType = entity.RequiredStorageType,
            TemperatureMinCelsius = entity.TemperatureMinCelsius,
            TemperatureMaxCelsius = entity.TemperatureMaxCelsius,
            ReorderLevel = entity.ReorderLevel,
            TargetStockLevel = entity.TargetStockLevel,
            UnitPrice = entity.UnitPrice,
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    protected override Task<List<ProductDto>> MapToGetListOutputDtosAsync(List<Product> entities)
    {
        var dtos = entities.Select(MapToGetOutputDto).ToList();
        return Task.FromResult(dtos);
    }
}
