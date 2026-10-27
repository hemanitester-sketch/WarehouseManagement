using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Outbound;

public interface IOutboundOrderAppService : IApplicationService
{
    Task<PagedResultDto<OutboundOrderDto>> GetListAsync(GetOutboundOrderListDto input);

    Task<OutboundOrderDto> GetAsync(Guid id);

    Task<OutboundOrderDto> CreateAsync(CreateUpdateOutboundOrderDto input);

    Task<OutboundOrderDto> UpdateAsync(Guid id, CreateUpdateOutboundOrderDto input);

    Task DeleteAsync(Guid id);

    Task<OutboundOrderDto> AllocateStockAsync(AllocateOrderStockDto input);

    Task<OutboundOrderDto> PickGoodsAsync(PickOrderGoodsDto input);

    Task<OutboundOrderDto> ShipOrderAsync(ShipOrderDto input);
}
