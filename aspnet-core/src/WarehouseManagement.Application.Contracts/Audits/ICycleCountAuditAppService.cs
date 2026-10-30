using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Audits;

public interface ICycleCountAuditAppService : IApplicationService
{
    Task<PagedResultDto<CycleCountAuditDto>> GetListAsync(GetCycleCountListDto input);

    Task<CycleCountAuditDto> GetAsync(Guid id);

    Task<CycleCountAuditDto> CreateAsync(CreateCycleCountAuditDto input);

    Task<CycleCountAuditDto> StartCountingAsync(Guid id);

    Task<CycleCountAuditDto> RecordCountsAsync(Guid id, List<RecordCountItemDto> items);

    Task<CycleCountAuditDto> SubmitForReviewAsync(Guid id, SubmitCountDto? input = null);

    Task<CycleCountAuditDto> RequestReCountAsync(Guid id, string? reason = null);

    Task<CycleCountAuditDto> ApproveAndReconcileAsync(Guid id, ApproveReconcileAuditDto? input = null);

    Task<CycleCountAuditDto> CancelAuditAsync(Guid id, string? reason = null);

    Task<CycleCountDashboardMetricsDto> GetDashboardMetricsAsync(Guid? warehouseId = null);

    Task<List<CreateCycleCountAuditItemDto>> GenerateSnapshotItemsAsync(
        Guid warehouseId,
        CycleCountType countType,
        Guid? zoneId = null,
        Guid? specificProductId = null);
}

