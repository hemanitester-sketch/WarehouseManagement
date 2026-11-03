using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Barcodes;

public interface ISupplierBarcodeAppService : IApplicationService
{
    Task<SupplierBarcodeMappingDto> CreateAsync(CreateSupplierBarcodeMappingDto input);

    Task<PagedResultDto<SupplierBarcodeMappingDto>> GetListAsync(GetSupplierBarcodeMappingsDto input);

    Task DeleteAsync(Guid id);
}

