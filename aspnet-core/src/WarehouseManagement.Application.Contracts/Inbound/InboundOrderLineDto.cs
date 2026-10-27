using System;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Inbound;

public class InboundOrderLineDto : EntityDto<Guid>
{
    public Guid InboundOrderId { get; set; }

    public Guid ProductId { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }

    public decimal ExpectedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal RemainingQuantity => Math.Max(0, ExpectedQuantity - ReceivedQuantity);

    public decimal UnitPrice { get; set; }
    public bool IsCompleted { get; set; }
}
