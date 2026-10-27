using System;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Warehouses;

public interface IWarehouseAppService :
    ICrudAppService<
        WarehouseDto,
        Guid,
        GetWarehouseListDto,
        CreateUpdateWarehouseDto,
        CreateUpdateWarehouseDto>
{
}
