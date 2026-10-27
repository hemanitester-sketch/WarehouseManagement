using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Outbound;

public class OutboundOrderLineDto : EntityDto<Guid>
{
    public Guid OutboundOrderId { get; set; }

    public Guid ProductId { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }

    public decimal OrderedQuantity { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal PickedQuantity { get; set; }
    public decimal RemainingToPickQuantity => Math.Max(0, OrderedQuantity - PickedQuantity);

    public decimal UnitPrice { get; set; }
    public bool IsCompleted { get; set; }
}
