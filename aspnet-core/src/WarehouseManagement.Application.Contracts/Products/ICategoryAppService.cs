using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Products;

public interface ICategoryAppService :
    ICrudAppService<
        CategoryDto,
        Guid,
        GetCategoryListDto,
        CreateUpdateCategoryDto,
        CreateUpdateCategoryDto>
{
    Task<ListResultDto<CategoryLookupDto>> GetLookupAsync();

    Task<CategoryDto?> FindByCodeAsync(string code);
}
