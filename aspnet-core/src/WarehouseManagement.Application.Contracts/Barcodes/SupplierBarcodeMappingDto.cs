using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Barcodes;

public class SupplierBarcodeMappingDto : FullAuditedEntityDto<Guid>
{
    public Guid SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public Guid ProductVariantId { get; set; }

    public string VariantSku { get; set; } = null!;

    public string VariantName { get; set; } = null!;

    public string SupplierBarcode { get; set; } = null!;

    public string? SupplierProductCode { get; set; }

    public BarcodePackagingLevel PackagingLevel { get; set; }

    public int UnitsPerPackage { get; set; }

    public bool IsActive { get; set; }
}

public class CreateSupplierBarcodeMappingDto
{
    [Required]
    public Guid SupplierId { get; set; }

    [Required]
    public Guid ProductVariantId { get; set; }

    [Required]
    [StringLength(BarcodeConsts.MaxBarcodeLength)]
    public string SupplierBarcode { get; set; } = null!;

    [StringLength(64)]
    public string? SupplierProductCode { get; set; }

    public BarcodePackagingLevel PackagingLevel { get; set; } = BarcodePackagingLevel.Each;

    public int UnitsPerPackage { get; set; } = 1;
}

public class GetSupplierBarcodeMappingsDto : PagedAndSortedResultRequestDto
{
    public Guid? SupplierId { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? ProductVariantId { get; set; }

    public string? Filter { get; set; }

    public bool? IsActive { get; set; }
}

