using System;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Warehouses;

public interface IWarehouseZoneAppService :
    ICrudAppService<
        WarehouseZoneDto,
        Guid,
        GetWarehouseZoneListDto,
        CreateUpdateWarehouseZoneDto,
        CreateUpdateWarehouseZoneDto>
{
}
