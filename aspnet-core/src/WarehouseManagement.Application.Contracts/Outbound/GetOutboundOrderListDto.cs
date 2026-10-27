using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Outbound;

public class GetOutboundOrderListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public Guid? WarehouseId { get; set; }

    public OutboundOrderStatus? Status { get; set; }

    public OrderPriority? Priority { get; set; }

    public DateTime? OrderDateFrom { get; set; }

    public DateTime? OrderDateTo { get; set; }
}
