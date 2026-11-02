using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Returns;

public class GetReturnOrderListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? SupplierId { get; set; }

    public ReturnType? ReturnType { get; set; }

    public ReturnOrderStatus? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
