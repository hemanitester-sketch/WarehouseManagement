using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Suppliers;

public class GetSupplierListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public bool? IsActive { get; set; }

    public string? City { get; set; }

    public string? PaymentTerms { get; set; }
}

