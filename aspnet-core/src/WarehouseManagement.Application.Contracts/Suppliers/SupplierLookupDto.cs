using System;

namespace WarehouseManagement.Suppliers;

public class SupplierLookupDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? City { get; set; }

    public string DisplayName => $"{Code} - {Name}";
}

