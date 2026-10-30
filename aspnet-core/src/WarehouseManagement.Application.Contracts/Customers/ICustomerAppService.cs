using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace WarehouseManagement.Customers;

public interface ICustomerAppService :
    ICrudAppService<
        CustomerDto,
        Guid,
        GetCustomerListDto,
        CreateUpdateCustomerDto,
        CreateUpdateCustomerDto>
{
    Task<ListResultDto<CustomerLookupDto>> GetLookupAsync();

    Task<CustomerDashboardMetricsDto> GetDashboardMetricsAsync();

    Task<CustomerDto> ToggleStatusAsync(Guid id, CustomerStatus newStatus);
}

