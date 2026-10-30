using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Customers;

public class GetCustomerListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public CustomerType? CustomerType { get; set; }

    public CustomerStatus? Status { get; set; }

    public string? City { get; set; }

    public string? PaymentTerms { get; set; }

    public bool? IsActive { get; set; }
}

