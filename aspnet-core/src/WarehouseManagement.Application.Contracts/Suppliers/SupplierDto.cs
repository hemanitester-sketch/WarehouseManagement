using System;
using System.Linq;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Suppliers;

public class SupplierDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? TaxNumber { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? PaymentTerms { get; set; }

    public bool IsActive { get; set; }

    public int? Rating { get; set; }

    public string? Notes { get; set; }

    public string FullAddress => string.Join(", ", new[] { Address, City, State, PostalCode }.Where(s => !string.IsNullOrWhiteSpace(s)));
}
