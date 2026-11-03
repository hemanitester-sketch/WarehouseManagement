using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WarehouseManagement.Barcodes;

namespace WarehouseManagement.Barcodes;

public class BarcodeLookupInput
{
    [Required]
    public string BarcodeValue { get; set; } = null!;

    public BarcodeTargetType? ExpectedTargetType { get; set; }

    public Guid? WarehouseId { get; set; }
}

public class BarcodeLookupCandidateDto
{
    public BarcodeTargetType TargetType { get; set; }

    public Guid TargetId { get; set; }

    public string Identifier { get; set; } = null!;

    public string DisplayText { get; set; } = null!;

    public Guid? ProductId { get; set; }

    public Guid? ProductVariantId { get; set; }
}

public class BarcodeLookupResultDto
{
    public bool IsFound { get; set; }

    public string ScannedValue { get; set; } = null!;

    public string NormalizedValue { get; set; } = null!;

    public BarcodeSymbology Symbology { get; set; }

    public BarcodeTargetType? TargetType { get; set; }

    public Guid? TargetId { get; set; }

    // Product & Variant details
    public Guid? ProductId { get; set; }

    public Guid? ProductVariantId { get; set; }

    public string? Sku { get; set; }

    public string? ProductName { get; set; }

    public string? VariantName { get; set; }

    public BarcodePackagingLevel? PackagingLevel { get; set; }

    public int? UnitsPerPackage { get; set; }

    // License Plate details
    public Guid? LicensePlateId { get; set; }

    public string? LicensePlateNumber { get; set; }

    // Location details
    public Guid? LocationId { get; set; }

    public string? LocationBarcode { get; set; }

    public string? LocationDisplayName { get; set; }

    // Lot details
    public Guid? LotId { get; set; }

    public string? LotBatchNumber { get; set; }

    public DateTime? ExpirationDate { get; set; }

    // Serial details
    public Guid? SerialId { get; set; }

    public string? SerialNumber { get; set; }

    // Supplier source details
    public bool IsSupplierBarcode { get; set; }

    public Guid? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public string? SupplierProductCode { get; set; }

    // Disambiguation
    public bool HasMultipleMatches => Candidates.Count > 1;

    public List<BarcodeLookupCandidateDto> Candidates { get; set; } = new();
}

