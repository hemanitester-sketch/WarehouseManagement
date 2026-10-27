using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Inventory;

public interface IInventoryAppService : IApplicationService
{
    Task<PagedResultDto<InventoryItemDto>> GetListAsync(GetInventoryListDto input);

    Task<InventoryItemDto> GetAsync(Guid id);

    Task<InventorySummaryDto> GetStockSummaryAsync(Guid warehouseId, Guid productId);

    Task<InventoryItemDto> AdjustStockAsync(AdjustStockDto input);

    Task<InventoryItemDto> TransferStockAsync(TransferStockDto input);

    Task<PagedResultDto<InventoryTransactionDto>> GetTransactionsAsync(GetInventoryTransactionListDto input);
}
