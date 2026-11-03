using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Barcodes;
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
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<ProductVariant, Guid> _variantRepository;
    private readonly IRepository<BarcodeIdentity, Guid> _barcodeIdentityRepository;
    private readonly IRepository<ProductBarcode, Guid> _productBarcodeRepository;
    private readonly IBarcodeGenerator _barcodeGenerator;

    public ProductAppService(
        IRepository<Product, Guid> repository,
        IRepository<Category, Guid> categoryRepository,
        IRepository<ProductVariant, Guid> variantRepository,
        IRepository<BarcodeIdentity, Guid> barcodeIdentityRepository,
        IRepository<ProductBarcode, Guid> productBarcodeRepository,
        IBarcodeGenerator barcodeGenerator)
        : base(repository)
    {
        _categoryRepository = categoryRepository;
        _variantRepository = variantRepository;
        _barcodeIdentityRepository = barcodeIdentityRepository;
        _productBarcodeRepository = productBarcodeRepository;
        _barcodeGenerator = barcodeGenerator;
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

        // For Simple products, automatically ensure a default variant exists
        if (entity.ProductType == ProductType.Simple)
        {
            var defaultVariant = new ProductVariant(
                GuidGenerator.Create(),
                entity.Id,
                entity.Sku,
                entity.Name,
                "DEFAULT",
                unitPrice: entity.UnitPrice,
                purchaseCost: 0,
                weightKg: entity.WeightKg,
                lengthCm: entity.LengthCm,
                widthCm: entity.WidthCm,
                heightCm: entity.HeightCm,
                imageUrl: entity.ImageUrl,
                isDefault: true,
                isActive: entity.IsActive,
                tenantId: CurrentTenant.Id
            );
            await _variantRepository.InsertAsync(defaultVariant, autoSave: true);

            var barcodeVal = !string.IsNullOrWhiteSpace(entity.Barcode)
                ? _barcodeGenerator.NormalizeBarcode(entity.Barcode)
                : await _barcodeGenerator.GenerateInternalCode128Async(BarcodeTargetType.ProductVariant, CurrentTenant.Id);

            if (string.IsNullOrWhiteSpace(entity.Barcode))
            {
                entity.Barcode = barcodeVal;
                await Repository.UpdateAsync(entity, autoSave: true);
            }

            var identity = new BarcodeIdentity(
                GuidGenerator.Create(),
                barcodeVal,
                BarcodeTargetType.ProductVariant,
                BarcodeKind.InternalCode128,
                _barcodeGenerator.DetectSymbology(barcodeVal),
                productVariantId: defaultVariant.Id,
                tenantId: CurrentTenant.Id
            );
            await _barcodeIdentityRepository.InsertAsync(identity, autoSave: true);

            var productBarcode = new ProductBarcode(
                GuidGenerator.Create(),
                identity.Id,
                entity.Id,
                defaultVariant.Id,
                BarcodePackagingLevel.Each,
                isPrimary: true,
                LabelPrintFormat.Zpl_4x6,
                CurrentTenant.Id
            );
            await _productBarcodeRepository.InsertAsync(productBarcode, autoSave: true);
        }

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

        return await GetAsync(entity.Id);
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
            .WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId!.Value)
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
            createInput.IsActive,
            createInput.CategoryId,
            createInput.ProductType,
            createInput.ManufacturerPartNumber
        );

        entity.ImageUrl = createInput.ImageUrl?.Trim();

        return Task.FromResult(entity);
    }

    protected override Task MapToEntityAsync(CreateUpdateProductDto updateInput, Product entity)
    {
        entity.SetSku(updateInput.Sku);
        entity.SetName(updateInput.Name);
        entity.Barcode = updateInput.Barcode?.Trim();
        entity.ProductType = updateInput.ProductType;
        entity.ManufacturerPartNumber = updateInput.ManufacturerPartNumber?.Trim();
        entity.Description = updateInput.Description;
        entity.Brand = updateInput.Brand;
        entity.ImageUrl = updateInput.ImageUrl?.Trim();
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
        entity.CategoryId = updateInput.CategoryId;

        return Task.CompletedTask;
    }

    protected override ProductDto MapToGetOutputDto(Product entity)
    {
        return new ProductDto
        {
            Id = entity.Id,
            Sku = entity.Sku,
            Name = entity.Name,
            ProductType = entity.ProductType,
            ManufacturerPartNumber = entity.ManufacturerPartNumber,
            Barcode = entity.Barcode,
            Description = entity.Description,
            Brand = entity.Brand,
            ImageUrl = entity.ImageUrl,
            Category = entity.Category,
            CategoryId = entity.CategoryId,
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
            VariantCount = entity.Variants?.Count ?? 0,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    public override async Task<ProductDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        var dto = MapToGetOutputDto(entity);
        dto.VariantCount = await _variantRepository.CountAsync(v => v.ProductId == id);
        if (entity.CategoryId.HasValue)
        {
            var cat = await _categoryRepository.FindAsync(entity.CategoryId.Value);
            if (cat != null)
            {
                dto.CategoryName = cat.Name;
                dto.CategoryCode = cat.Code;
            }
        }
        return dto;
    }

    protected override async Task<List<ProductDto>> MapToGetListOutputDtosAsync(List<Product> entities)
    {
        if (entities.Count == 0)
        {
            return new List<ProductDto>();
        }

        var categoryIds = entities
            .Where(e => e.CategoryId.HasValue)
            .Select(e => e.CategoryId!.Value)
            .Distinct()
            .ToList();

        var categories = categoryIds.Count > 0
            ? await _categoryRepository.GetListAsync(c => categoryIds.Contains(c.Id))
            : new List<Category>();

        var catMap = categories.ToDictionary(c => c.Id);

        var productIds = entities.Select(e => e.Id).ToList();
        var variantQuery = await _variantRepository.GetQueryableAsync();
        var countsList = await AsyncExecuter.ToListAsync(
            variantQuery
                .Where(v => productIds.Contains(v.ProductId))
                .GroupBy(v => v.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Count() })
        );
        var variantCounts = countsList.ToDictionary(x => x.ProductId, x => x.Count);

        var dtos = entities.Select(e =>
        {
            var dto = MapToGetOutputDto(e);
            if (e.CategoryId.HasValue && catMap.TryGetValue(e.CategoryId.Value, out var cat))
            {
                dto.CategoryName = cat.Name;
                dto.CategoryCode = cat.Code;
            }
            if (variantCounts.TryGetValue(e.Id, out var count))
            {
                dto.VariantCount = count;
            }
            return dto;
        }).ToList();

        return dtos;
    }
}
