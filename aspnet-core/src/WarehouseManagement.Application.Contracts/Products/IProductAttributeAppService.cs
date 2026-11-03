using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Products;

public interface IProductAttributeAppService : IApplicationService
{
    Task<List<ProductAttributeDto>> GetListAsync();

    Task<ProductAttributeDto> GetAsync(Guid id);

    Task<ProductAttributeDto> CreateAsync(CreateUpdateProductAttributeDto input);

    Task<ProductAttributeDto> UpdateAsync(Guid id, CreateUpdateProductAttributeDto input);

    Task DeleteAsync(Guid id);

    Task<List<CategoryAttributeTemplateDto>> GetAttributesByCategoryIdAsync(Guid categoryId);

    Task AssignAttributeToCategoryAsync(AssignCategoryAttributeDto input);

    Task RemoveAttributeFromCategoryAsync(Guid categoryId, Guid attributeId);
}

