using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Reports;

public interface IWarehouseReportAppService : IApplicationService
{
    Task<ExecutiveWarehouseKpisDto> GetExecutiveKpisAsync(GetWarehouseReportFilterDto input);

    Task<List<StockAgingReportItemDto>> GetStockAgingReportAsync(GetWarehouseReportFilterDto input);

    Task<List<SpaceUtilizationReportItemDto>> GetSpaceUtilizationReportAsync(GetWarehouseReportFilterDto input);

    Task<List<InboundSupplierSlaReportItemDto>> GetSupplierSlaReportAsync(GetWarehouseReportFilterDto input);

    Task<OutboundFulfillmentReportItemDto> GetOutboundFulfillmentReportAsync(GetWarehouseReportFilterDto input);

    Task<QualityReverseLogisticsReportDto> GetQualityAndReturnsReportAsync(GetWarehouseReportFilterDto input);
}
