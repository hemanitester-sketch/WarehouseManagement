using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Returns;

public interface IReturnOrderAppService : IApplicationService
{
    Task<PagedResultDto<ReturnOrderDto>> GetListAsync(GetReturnOrderListDto input);

    Task<ReturnOrderDto> GetAsync(Guid id);

    Task<ReturnOrderDto> CreateAsync(CreateReturnOrderDto input);

    Task<ReturnOrderDto> ApproveAsync(Guid id);

    Task<ReturnOrderDto> MarkInTransitAsync(Guid id, string? carrierName, string? trackingNumber);

    Task<ReturnOrderDto> ReceiveAsync(Guid id, ReceiveReturnDto input);

    Task<ReturnOrderDto> InspectAndResolveAsync(Guid id, InspectReturnOrderDto input);

    Task<ReturnOrderDto> RejectAsync(Guid id, string? reason);

    Task<ReturnOrderDto> CancelAsync(Guid id, string? reason);

    Task<ReturnDashboardMetricsDto> GetDashboardMetricsAsync(Guid? warehouseId = null);
}
