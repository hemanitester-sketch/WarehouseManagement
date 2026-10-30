using System;

namespace WarehouseManagement.Customers;

public class CustomerLookupDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public CustomerType CustomerType { get; set; }

    public string? ContactPerson { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? ShippingAddress { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? PaymentTerms { get; set; }

    public decimal CreditLimit { get; set; }

    public decimal CurrentOutstanding { get; set; }
}

