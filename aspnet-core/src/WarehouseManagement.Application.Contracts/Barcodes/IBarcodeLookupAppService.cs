using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Barcodes;

public interface IBarcodeLookupAppService : IApplicationService
{
    Task<BarcodeLookupResultDto> ResolveBarcodeAsync(BarcodeLookupInput input);
}

