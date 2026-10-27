using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Warehouses;

public interface IWarehouseLocationAppService :
    ICrudAppService<
        WarehouseLocationDto,
        Guid,
        GetWarehouseLocationListDto,
        CreateUpdateWarehouseLocationDto,
        CreateUpdateWarehouseLocationDto>
{
    Task<WarehouseLocationDto?> FindByBarcodeAsync(string barcode);
}
