using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Products;

public interface IProductAppService :
    ICrudAppService<
        ProductDto,
        Guid,
        GetProductListDto,
        CreateUpdateProductDto,
        CreateUpdateProductDto>
{
    Task<ProductDto?> FindBySkuAsync(string sku);

    Task<ProductDto?> FindByBarcodeAsync(string barcode);
}
