using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace WarehouseManagement.Products;

public class ProductAttributeAppService : ApplicationService, IProductAttributeAppService
{
    private readonly IRepository<ProductAttribute, Guid> _attributeRepository;
    private readonly IRepository<ProductAttributeValue, Guid> _attributeValueRepository;
    private readonly IRepository<CategoryAttributeTemplate, Guid> _categoryTemplateRepository;

    public ProductAttributeAppService(
        IRepository<ProductAttribute, Guid> attributeRepository,
        IRepository<ProductAttributeValue, Guid> attributeValueRepository,
        IRepository<CategoryAttributeTemplate, Guid> categoryTemplateRepository)
    {
        _attributeRepository = attributeRepository;
        _attributeValueRepository = attributeValueRepository;
        _categoryTemplateRepository = categoryTemplateRepository;
    }

    public async Task<List<ProductAttributeDto>> GetListAsync()
    {
        var attrQuery = (await _attributeRepository.WithDetailsAsync(x => x.Values))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name);

        var attributes = await AsyncExecuter.ToListAsync(attrQuery);
        var changed = false;

        if (!attributes.Any(a => a.Code == "SIZE"))
        {
            var sizeAttr = new ProductAttribute(
                GuidGenerator.Create(),
                "SIZE",
                "Size",
                AttributeDataType.Dropdown,
                isVariantDefining: true,
                isMandatory: false,
                isActive: true,
                displayOrder: 1,
                tenantId: CurrentTenant.Id
            );
            sizeAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), sizeAttr.Id, "S", "Small", null, 1, true, CurrentTenant.Id));
            sizeAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), sizeAttr.Id, "M", "Medium", null, 2, true, CurrentTenant.Id));
            sizeAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), sizeAttr.Id, "L", "Large", null, 3, true, CurrentTenant.Id));
            sizeAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), sizeAttr.Id, "XL", "Extra Large", null, 4, true, CurrentTenant.Id));
            sizeAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), sizeAttr.Id, "XXL", "Double XL", null, 5, true, CurrentTenant.Id));
            await _attributeRepository.InsertAsync(sizeAttr, autoSave: true);
            changed = true;
        }

        if (!attributes.Any(a => a.Code == "COLOR"))
        {
            var colorAttr = new ProductAttribute(
                GuidGenerator.Create(),
                "COLOR",
                "Color",
                AttributeDataType.Dropdown,
                isVariantDefining: true,
                isMandatory: false,
                isActive: true,
                displayOrder: 2,
                tenantId: CurrentTenant.Id
            );
            colorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), colorAttr.Id, "Black", "Black", "#000000", 1, true, CurrentTenant.Id));
            colorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), colorAttr.Id, "White", "White", "#ffffff", 2, true, CurrentTenant.Id));
            colorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), colorAttr.Id, "Navy Blue", "Navy", "#000080", 3, true, CurrentTenant.Id));
            colorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), colorAttr.Id, "Red", "Red", "#ff0000", 4, true, CurrentTenant.Id));
            colorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), colorAttr.Id, "Olive Green", "Green", "#2e7d32", 5, true, CurrentTenant.Id));
            await _attributeRepository.InsertAsync(colorAttr, autoSave: true);
            changed = true;
        }

        if (!attributes.Any(a => a.Code == "FABRIC"))
        {
            var fabricAttr = new ProductAttribute(
                GuidGenerator.Create(),
                "FABRIC",
                "Fabric",
                AttributeDataType.Dropdown,
                isVariantDefining: true,
                isMandatory: false,
                isActive: true,
                displayOrder: 3,
                tenantId: CurrentTenant.Id
            );
            fabricAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), fabricAttr.Id, "100% Cotton", "Cotton", null, 1, true, CurrentTenant.Id));
            fabricAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), fabricAttr.Id, "Polyester Blend", "Polyester", null, 2, true, CurrentTenant.Id));
            fabricAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), fabricAttr.Id, "Denim", "Denim", null, 3, true, CurrentTenant.Id));
            fabricAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), fabricAttr.Id, "Linen", "Linen", null, 4, true, CurrentTenant.Id));
            await _attributeRepository.InsertAsync(fabricAttr, autoSave: true);
            changed = true;
        }

        if (!attributes.Any(a => a.Code == "MATERIAL"))
        {
            var matAttr = new ProductAttribute(
                GuidGenerator.Create(),
                "MATERIAL",
                "Material",
                AttributeDataType.Dropdown,
                isVariantDefining: true,
                isMandatory: false,
                isActive: true,
                displayOrder: 4,
                tenantId: CurrentTenant.Id
            );
            matAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), matAttr.Id, "Stainless Steel", "Steel", null, 1, true, CurrentTenant.Id));
            matAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), matAttr.Id, "Plastic", "Plastic", null, 2, true, CurrentTenant.Id));
            matAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), matAttr.Id, "Ceramic", "Ceramic", null, 3, true, CurrentTenant.Id));
            await _attributeRepository.InsertAsync(matAttr, autoSave: true);
            changed = true;
        }

        if (!attributes.Any(a => a.Code == "FLAVOR"))
        {
            var flavorAttr = new ProductAttribute(
                GuidGenerator.Create(),
                "FLAVOR",
                "Flavor",
                AttributeDataType.Dropdown,
                isVariantDefining: true,
                isMandatory: false,
                isActive: true,
                displayOrder: 5,
                tenantId: CurrentTenant.Id
            );
            flavorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), flavorAttr.Id, "Orange", "Orange", null, 1, true, CurrentTenant.Id));
            flavorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), flavorAttr.Id, "Mint", "Mint", null, 2, true, CurrentTenant.Id));
            flavorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), flavorAttr.Id, "Chocolate", "Chocolate", null, 3, true, CurrentTenant.Id));
            await _attributeRepository.InsertAsync(flavorAttr, autoSave: true);
            changed = true;
        }

        if (changed)
        {
            var refreshedQuery = (await _attributeRepository.WithDetailsAsync(x => x.Values))
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name);
            attributes = await AsyncExecuter.ToListAsync(refreshedQuery);
        }

        return attributes.Select(MapToDto).ToList();
    }

    private async Task SeedDefaultAttributesAsync()
    {
        var sizeAttr = new ProductAttribute(
            GuidGenerator.Create(),
            "SIZE",
            "Size",
            AttributeDataType.Dropdown,
            isVariantDefining: true,
            isMandatory: false,
            isActive: true,
            displayOrder: 1,
            tenantId: CurrentTenant.Id
        );
        sizeAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), sizeAttr.Id, "S", "Small", null, 1, true, CurrentTenant.Id));
        sizeAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), sizeAttr.Id, "M", "Medium", null, 2, true, CurrentTenant.Id));
        sizeAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), sizeAttr.Id, "L", "Large", null, 3, true, CurrentTenant.Id));
        sizeAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), sizeAttr.Id, "XL", "Extra Large", null, 4, true, CurrentTenant.Id));

        var colorAttr = new ProductAttribute(
            GuidGenerator.Create(),
            "COLOR",
            "Color",
            AttributeDataType.Dropdown,
            isVariantDefining: true,
            isMandatory: false,
            isActive: true,
            displayOrder: 2,
            tenantId: CurrentTenant.Id
        );
        colorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), colorAttr.Id, "Black", "Black", "#000000", 1, true, CurrentTenant.Id));
        colorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), colorAttr.Id, "White", "White", "#ffffff", 2, true, CurrentTenant.Id));
        colorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), colorAttr.Id, "Blue", "Blue", "#0000ff", 3, true, CurrentTenant.Id));
        colorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), colorAttr.Id, "Red", "Red", "#ff0000", 4, true, CurrentTenant.Id));

        var flavorAttr = new ProductAttribute(
            GuidGenerator.Create(),
            "FLAVOR",
            "Flavor",
            AttributeDataType.Dropdown,
            isVariantDefining: true,
            isMandatory: false,
            isActive: true,
            displayOrder: 3,
            tenantId: CurrentTenant.Id
        );
        flavorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), flavorAttr.Id, "Orange", "Orange", null, 1, true, CurrentTenant.Id));
        flavorAttr.Values.Add(new ProductAttributeValue(GuidGenerator.Create(), flavorAttr.Id, "Mint", "Mint", null, 2, true, CurrentTenant.Id));

        await _attributeRepository.InsertAsync(sizeAttr, autoSave: true);
        await _attributeRepository.InsertAsync(colorAttr, autoSave: true);
        await _attributeRepository.InsertAsync(flavorAttr, autoSave: true);
    }

    public async Task<ProductAttributeDto> GetAsync(Guid id)
    {
        var attrQuery = (await _attributeRepository.WithDetailsAsync(x => x.Values))
            .Where(x => x.Id == id);

        var attribute = await AsyncExecuter.FirstOrDefaultAsync(attrQuery);
        if (attribute == null)
        {
            throw new UserFriendlyException("Attribute not found.");
        }

        return MapToDto(attribute);
    }

    public async Task<ProductAttributeDto> CreateAsync(CreateUpdateProductAttributeDto input)
    {
        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        var attrQuery = (await _attributeRepository.GetQueryableAsync())
            .Where(x => x.Code == normalizedCode);

        var exists = await AsyncExecuter.AnyAsync(attrQuery);
        if (exists)
        {
            throw new UserFriendlyException($"An attribute with code '{normalizedCode}' already exists.");
        }

        var attribute = new ProductAttribute(
            GuidGenerator.Create(),
            normalizedCode,
            input.Name.Trim(),
            input.DataType,
            isVariantDefining: input.IsVariantDefining,
            isMandatory: input.IsRequired,
            isActive: input.IsActive,
            displayOrder: input.DisplayOrder,
            tenantId: CurrentTenant.Id
        );

        if (input.Values.Count > 0)
        {
            foreach (var val in input.Values)
            {
                attribute.Values.Add(new ProductAttributeValue(
                    GuidGenerator.Create(),
                    attribute.Id,
                    val.Value.Trim(),
                    val.DisplayLabel?.Trim(),
                    val.ColorHex?.Trim(),
                    val.DisplayOrder,
                    isActive: true,
                    tenantId: CurrentTenant.Id
                ));
            }
        }

        await _attributeRepository.InsertAsync(attribute, autoSave: true);
        return MapToDto(attribute);
    }

    public async Task<ProductAttributeDto> UpdateAsync(Guid id, CreateUpdateProductAttributeDto input)
    {
        var attrQuery = (await _attributeRepository.WithDetailsAsync(x => x.Values))
            .Where(x => x.Id == id);

        var attribute = await AsyncExecuter.FirstOrDefaultAsync(attrQuery);
        if (attribute == null)
        {
            throw new UserFriendlyException("Attribute not found.");
        }

        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        if (attribute.Code != normalizedCode)
        {
            var existsQuery = (await _attributeRepository.GetQueryableAsync())
                .Where(x => x.Code == normalizedCode && x.Id != id);
            var exists = await AsyncExecuter.AnyAsync(existsQuery);
            if (exists)
            {
                throw new UserFriendlyException($"An attribute with code '{normalizedCode}' already exists.");
            }
            attribute.SetCode(normalizedCode);
        }

        attribute.SetName(input.Name.Trim());
        attribute.DataType = input.DataType;
        attribute.IsVariantDefining = input.IsVariantDefining;
        attribute.IsMandatory = input.IsRequired;
        attribute.DisplayOrder = input.DisplayOrder;
        attribute.IsActive = input.IsActive;

        // Sync values
        var existingValueMap = attribute.Values.ToDictionary(v => v.Value.ToUpperInvariant());
        var newValues = new List<ProductAttributeValue>();

        foreach (var v in input.Values)
        {
            var key = v.Value.Trim().ToUpperInvariant();
            if (existingValueMap.TryGetValue(key, out var existing))
            {
                existing.DisplayCode = v.DisplayLabel?.Trim();
                existing.ColorHexCode = v.ColorHex?.Trim();
                existing.DisplayOrder = v.DisplayOrder;
                newValues.Add(existing);
            }
            else
            {
                newValues.Add(new ProductAttributeValue(
                    GuidGenerator.Create(),
                    attribute.Id,
                    v.Value.Trim(),
                    v.DisplayLabel?.Trim(),
                    v.ColorHex?.Trim(),
                    v.DisplayOrder,
                    isActive: true,
                    tenantId: CurrentTenant.Id
                ));
            }
        }

        attribute.Values.Clear();
        foreach (var item in newValues)
        {
            attribute.Values.Add(item);
        }

        await _attributeRepository.UpdateAsync(attribute, autoSave: true);
        return MapToDto(attribute);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _attributeRepository.DeleteAsync(id);
    }

    public async Task<List<CategoryAttributeTemplateDto>> GetAttributesByCategoryIdAsync(Guid categoryId)
    {
        var templateQuery = (await _categoryTemplateRepository.GetQueryableAsync())
            .Where(x => x.CategoryId == categoryId)
            .OrderBy(x => x.DisplayOrder);

        var templates = await AsyncExecuter.ToListAsync(templateQuery);
        if (templates.Count == 0)
        {
            return new List<CategoryAttributeTemplateDto>();
        }

        var attrIds = templates.Select(t => t.ProductAttributeId).Distinct().ToList();
        var attrQuery = (await _attributeRepository.WithDetailsAsync(a => a.Values))
            .Where(a => attrIds.Contains(a.Id));

        var attributes = (await AsyncExecuter.ToListAsync(attrQuery)).ToDictionary(a => a.Id);

        var result = new List<CategoryAttributeTemplateDto>();
        foreach (var t in templates)
        {
            if (attributes.TryGetValue(t.ProductAttributeId, out var a))
            {
                result.Add(new CategoryAttributeTemplateDto
                {
                    Id = t.Id,
                    CategoryId = t.CategoryId,
                    AttributeId = t.ProductAttributeId,
                    AttributeName = a.Name,
                    AttributeCode = a.Code,
                    DataType = a.DataType,
                    IsVariantDefining = a.IsVariantDefining,
                    IsRequired = t.IsRequired,
                    DisplayOrder = t.DisplayOrder,
                    PredefinedValues = a.Values
                        .Where(v => v.IsActive)
                        .OrderBy(v => v.DisplayOrder)
                        .Select(v => new ProductAttributeValueDto
                        {
                            Id = v.Id,
                            AttributeId = v.ProductAttributeId,
                            Value = v.Value,
                            DisplayLabel = v.DisplayCode,
                            ColorHex = v.ColorHexCode,
                            DisplayOrder = v.DisplayOrder,
                            IsActive = v.IsActive
                        }).ToList()
                });
            }
        }

        return result;
    }

    public async Task AssignAttributeToCategoryAsync(AssignCategoryAttributeDto input)
    {
        var query = (await _categoryTemplateRepository.GetQueryableAsync())
            .Where(x => x.CategoryId == input.CategoryId && x.ProductAttributeId == input.AttributeId);

        var exists = await AsyncExecuter.AnyAsync(query);
        if (exists)
        {
            return;
        }

        var template = new CategoryAttributeTemplate(
            GuidGenerator.Create(),
            input.CategoryId,
            input.AttributeId,
            isRequired: input.IsRequired,
            displayOrder: input.DisplayOrder,
            tenantId: CurrentTenant.Id
        );

        await _categoryTemplateRepository.InsertAsync(template, autoSave: true);
    }

    public async Task RemoveAttributeFromCategoryAsync(Guid categoryId, Guid attributeId)
    {
        var query = (await _categoryTemplateRepository.GetQueryableAsync())
            .Where(x => x.CategoryId == categoryId && x.ProductAttributeId == attributeId);

        var template = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (template != null)
        {
            await _categoryTemplateRepository.DeleteAsync(template, autoSave: true);
        }
    }

    private static ProductAttributeDto MapToDto(ProductAttribute entity)
    {
        return new ProductAttributeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            DataType = entity.DataType,
            IsVariantDefining = entity.IsVariantDefining,
            IsRequired = entity.IsMandatory,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            Values = entity.Values
                .OrderBy(v => v.DisplayOrder)
                .Select(v => new ProductAttributeValueDto
                {
                    Id = v.Id,
                    AttributeId = v.ProductAttributeId,
                    Value = v.Value,
                    DisplayLabel = v.DisplayCode,
                    ColorHex = v.ColorHexCode,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive
                }).ToList()
        };
    }
}
