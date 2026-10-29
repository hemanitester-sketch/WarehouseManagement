using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Suppliers;

public interface ISupplierAppService :
    ICrudAppService<
        SupplierDto,
        Guid,
        GetSupplierListDto,
        CreateUpdateSupplierDto,
        CreateUpdateSupplierDto>
{
    Task<ListResultDto<SupplierLookupDto>> GetLookupAsync();
}

