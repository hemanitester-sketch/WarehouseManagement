using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Inbound;

public class GetInboundOrderListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public Guid? WarehouseId { get; set; }

    public InboundOrderStatus? Status { get; set; }

    public DateTime? ExpectedDateFrom { get; set; }

    public DateTime? ExpectedDateTo { get; set; }
}
