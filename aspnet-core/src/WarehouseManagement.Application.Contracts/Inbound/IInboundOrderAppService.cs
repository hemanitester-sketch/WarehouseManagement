using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Inbound;

public interface IInboundOrderAppService : IApplicationService
{
    Task<PagedResultDto<InboundOrderDto>> GetListAsync(GetInboundOrderListDto input);

    Task<InboundOrderDto> GetAsync(Guid id);

    Task<InboundOrderDto> CreateAsync(CreateUpdateInboundOrderDto input);

    Task<InboundOrderDto> UpdateAsync(Guid id, CreateUpdateInboundOrderDto input);

    Task DeleteAsync(Guid id);

    Task<InboundOrderDto> ReceiveGoodsAsync(ReceiveInboundGoodsDto input);
}
