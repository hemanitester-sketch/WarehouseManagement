using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Transfers;

public interface IStockTransferAppService : IApplicationService
{
    Task<PagedResultDto<StockTransferDto>> GetListAsync(GetStockTransferListDto input);

    Task<StockTransferDto> GetAsync(Guid id);

    Task<StockTransferDto> CreateAsync(CreateUpdateStockTransferDto input);

    Task<StockTransferDto> ApproveAsync(Guid id);

    Task<StockTransferDto> DispatchAsync(Guid id, DispatchTransferDto input);

    Task<StockTransferDto> CompleteAsync(Guid id);

    Task<StockTransferDto> CancelAsync(Guid id, string? reason = null);

    Task DeleteAsync(Guid id);
}

