using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Transfers;

public class GetStockTransferListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public TransferType? TransferType { get; set; }

    public TransferStatus? Status { get; set; }

    public Guid? SourceWarehouseId { get; set; }

    public Guid? DestinationWarehouseId { get; set; }
}

