using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace WarehouseManagement.Products;

public class CategoryAppService :
    CrudAppService<
        Category,
        CategoryDto,
        Guid,
        GetCategoryListDto,
        CreateUpdateCategoryDto,
        CreateUpdateCategoryDto>,
    ICategoryAppService
{
    public CategoryAppService(IRepository<Category, Guid> repository)
        : base(repository)
    {
        GetPolicyName = null;
        GetListPolicyName = null;
        CreatePolicyName = null;
        UpdatePolicyName = null;
        DeletePolicyName = null;
    }

    public override async Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto input)
    {
        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        var exists = await Repository.AnyAsync(c => c.Code == normalizedCode);
        if (exists)
        {
            throw new UserFriendlyException($"A category with code '{normalizedCode}' already exists. Please choose a unique code.");
        }

        if (input.ParentCategoryId.HasValue)
        {
            var parent = await Repository.FindAsync(input.ParentCategoryId.Value);
            if (parent == null)
            {
                throw new UserFriendlyException("The selected parent category does not exist.");
            }
        }

        var entity = await MapToEntityAsync(input);
        TryToSetTenantId(entity);
        await Repository.InsertAsync(entity, autoSave: true);

        return await GetAsync(entity.Id);
    }

    public override async Task<CategoryDto> UpdateAsync(Guid id, CreateUpdateCategoryDto input)
    {
        var entity = await Repository.GetAsync(id);

        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        var exists = await Repository.AnyAsync(c => c.Id != id && c.Code == normalizedCode);
        if (exists)
        {
            throw new UserFriendlyException($"A category with code '{normalizedCode}' already exists. Please choose a unique code.");
        }

        if (input.ParentCategoryId.HasValue)
        {
            if (input.ParentCategoryId.Value == id)
            {
                throw new UserFriendlyException("A category cannot be its own parent category.");
            }

            var parent = await Repository.FindAsync(input.ParentCategoryId.Value);
            if (parent == null)
            {
                throw new UserFriendlyException("The selected parent category does not exist.");
            }
        }

        entity.SetCode(input.Code);
        entity.SetName(input.Name);
        entity.Description = input.Description;
        entity.ParentCategoryId = input.ParentCategoryId;
        entity.IsActive = input.IsActive;

        await Repository.UpdateAsync(entity, autoSave: true);

        return await GetAsync(entity.Id);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var hasChildren = await Repository.AnyAsync(c => c.ParentCategoryId == id);
        if (hasChildren)
        {
            throw new UserFriendlyException("Cannot delete this category because it has subcategories. Please reassign or delete its subcategories first.");
        }

        await base.DeleteAsync(id);
    }

    public override async Task<CategoryDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        var dto = MapToGetOutputDto(entity);

        if (entity.ParentCategoryId.HasValue)
        {
            var parent = await Repository.FindAsync(entity.ParentCategoryId.Value);
            if (parent != null)
            {
                dto.ParentCategoryName = parent.Name;
                dto.ParentCategoryCode = parent.Code;
            }
        }

        dto.SubCategoriesCount = await Repository.CountAsync(c => c.ParentCategoryId == id);

        return dto;
    }

    public async Task<CategoryDto?> FindByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var normalized = code.Trim().ToUpperInvariant();
        var entity = await Repository.FirstOrDefaultAsync(c => c.Code == normalized);
        return entity == null ? null : await GetAsync(entity.Id);
    }

    public async Task<ListResultDto<CategoryLookupDto>> GetLookupAsync()
    {
        var categories = await Repository.GetListAsync(c => c.IsActive);
        var parentMap = categories.ToDictionary(c => c.Id);

        var lookups = categories
            .OrderBy(c => c.ParentCategoryId.HasValue ? 1 : 0)
            .ThenBy(c => c.Name)
            .Select(c =>
            {
                string? parentName = null;
                string displayName = c.Name;

                if (c.ParentCategoryId.HasValue && parentMap.TryGetValue(c.ParentCategoryId.Value, out var parent))
                {
                    parentName = parent.Name;
                    displayName = $"{parent.Name} > {c.Name}";
                }

                return new CategoryLookupDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = parentName,
                    DisplayName = displayName
                };
            })
            .OrderBy(c => c.DisplayName)
            .ToList();

        return new ListResultDto<CategoryLookupDto>(lookups);
    }

    protected override async Task<IQueryable<Category>> CreateFilteredQueryAsync(GetCategoryListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.Code.Contains(input.Filter!) || x.Name.Contains(input.Filter!))
            .WhereIf(input.ParentCategoryId.HasValue, x => x.ParentCategoryId == input.ParentCategoryId!.Value)
            .WhereIf(input.IsRootOnly == true, x => x.ParentCategoryId == null)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);
    }

    protected override async Task<List<CategoryDto>> MapToGetListOutputDtosAsync(List<Category> entities)
    {
        if (entities.Count == 0)
        {
            return new List<CategoryDto>();
        }

        var allCategories = await Repository.GetListAsync();
        var categoryMap = allCategories.ToDictionary(c => c.Id);
        var subCounts = allCategories
            .Where(c => c.ParentCategoryId.HasValue)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var dtos = entities.Select(entity =>
        {
            var dto = MapToGetOutputDto(entity);
            if (entity.ParentCategoryId.HasValue && categoryMap.TryGetValue(entity.ParentCategoryId.Value, out var parent))
            {
                dto.ParentCategoryName = parent.Name;
                dto.ParentCategoryCode = parent.Code;
            }
            dto.SubCategoriesCount = subCounts.GetValueOrDefault(entity.Id, 0);
            return dto;
        }).ToList();

        return dtos;
    }

    protected override Task<Category> MapToEntityAsync(CreateUpdateCategoryDto createInput)
    {
        var entity = new Category(
            GuidGenerator.Create(),
            createInput.Code,
            createInput.Name,
            createInput.ParentCategoryId,
            createInput.Description,
            createInput.IsActive,
            CurrentTenant.Id
        );

        return Task.FromResult(entity);
    }

    protected override Task MapToEntityAsync(CreateUpdateCategoryDto updateInput, Category entity)
    {
        entity.SetCode(updateInput.Code);
        entity.SetName(updateInput.Name);
        entity.Description = updateInput.Description;
        entity.ParentCategoryId = updateInput.ParentCategoryId;
        entity.IsActive = updateInput.IsActive;

        return Task.CompletedTask;
    }

    protected override CategoryDto MapToGetOutputDto(Category entity)
    {
        return new CategoryDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            ParentCategoryId = entity.ParentCategoryId,
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }
}
