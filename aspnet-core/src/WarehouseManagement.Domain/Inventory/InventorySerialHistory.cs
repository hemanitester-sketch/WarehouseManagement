using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace WarehouseManagement.Inventory;

public class InventorySerialHistory : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid SerialId { get; set; }

    public SerialEventType EventType { get; set; }

    public Guid? FromLocationId { get; set; }

    public Guid? ToLocationId { get; set; }

    public Guid? FromLicensePlateId { get; set; }

    public Guid? ToLicensePlateId { get; set; }

    public string? ReferenceDocumentNumber { get; set; }

    public string? Notes { get; set; }

    public DateTime EventTime { get; set; }

    public InventorySerial Serial { get; set; } = null!;

    public InventorySerialHistory()
    {
    }

    public InventorySerialHistory(
        Guid id,
        Guid serialId,
        SerialEventType eventType,
        Guid? fromLocationId = null,
        Guid? toLocationId = null,
        Guid? fromLicensePlateId = null,
        Guid? toLicensePlateId = null,
        string? referenceDocumentNumber = null,
        string? notes = null,
        DateTime? eventTime = null,
        Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        SerialId = serialId;
        EventType = eventType;
        FromLocationId = fromLocationId;
        ToLocationId = toLocationId;
        FromLicensePlateId = fromLicensePlateId;
        ToLicensePlateId = toLicensePlateId;
        ReferenceDocumentNumber = referenceDocumentNumber?.Trim();
        Notes = notes?.Trim();
        EventTime = eventTime ?? DateTime.UtcNow;
    }
}

