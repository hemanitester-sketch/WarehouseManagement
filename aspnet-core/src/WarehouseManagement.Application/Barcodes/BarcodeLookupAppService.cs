using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Inventory;
using WarehouseManagement.Products;
using WarehouseManagement.Suppliers;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Barcodes;

public class BarcodeLookupAppService : ApplicationService, IBarcodeLookupAppService
{
    private readonly IReadOnlyRepository<BarcodeIdentity, Guid> _barcodeIdentityRepository;
    private readonly IReadOnlyRepository<SupplierBarcodeMapping, Guid> _supplierMappingRepository;
    private readonly IReadOnlyRepository<ProductVariant, Guid> _variantRepository;
    private readonly IReadOnlyRepository<Product, Guid> _productRepository;
    private readonly IReadOnlyRepository<ProductBarcode, Guid> _productBarcodeRepository;
    private readonly IReadOnlyRepository<LicensePlate, Guid> _licensePlateRepository;
    private readonly IReadOnlyRepository<WarehouseLocation, Guid> _locationRepository;
    private readonly IReadOnlyRepository<InventoryLot, Guid> _lotRepository;
    private readonly IReadOnlyRepository<InventorySerial, Guid> _serialRepository;
    private readonly IReadOnlyRepository<Supplier, Guid> _supplierRepository;
    private readonly IBarcodeGenerator _barcodeGenerator;

    public BarcodeLookupAppService(
        IReadOnlyRepository<BarcodeIdentity, Guid> barcodeIdentityRepository,
        IReadOnlyRepository<SupplierBarcodeMapping, Guid> supplierMappingRepository,
        IReadOnlyRepository<ProductVariant, Guid> variantRepository,
        IReadOnlyRepository<Product, Guid> productRepository,
        IReadOnlyRepository<ProductBarcode, Guid> productBarcodeRepository,
        IReadOnlyRepository<LicensePlate, Guid> licensePlateRepository,
        IReadOnlyRepository<WarehouseLocation, Guid> locationRepository,
        IReadOnlyRepository<InventoryLot, Guid> lotRepository,
        IReadOnlyRepository<InventorySerial, Guid> serialRepository,
        IReadOnlyRepository<Supplier, Guid> supplierRepository,
        IBarcodeGenerator barcodeGenerator)
    {
        _barcodeIdentityRepository = barcodeIdentityRepository;
        _supplierMappingRepository = supplierMappingRepository;
        _variantRepository = variantRepository;
        _productRepository = productRepository;
        _productBarcodeRepository = productBarcodeRepository;
        _licensePlateRepository = licensePlateRepository;
        _locationRepository = locationRepository;
        _lotRepository = lotRepository;
        _serialRepository = serialRepository;
        _supplierRepository = supplierRepository;
        _barcodeGenerator = barcodeGenerator;
    }

    public async Task<BarcodeLookupResultDto> ResolveBarcodeAsync(BarcodeLookupInput input)
    {
        var scanned = input.BarcodeValue ?? string.Empty;
        var normalized = _barcodeGenerator.NormalizeBarcode(scanned);
        var symbology = _barcodeGenerator.DetectSymbology(normalized);

        var result = new BarcodeLookupResultDto
        {
            ScannedValue = scanned,
            NormalizedValue = normalized,
            Symbology = symbology,
            IsFound = false
        };

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return result;
        }

        // 1. Query Canonical BarcodeIdentity
        var identityQuery = (await _barcodeIdentityRepository.GetQueryableAsync())
            .Where(x => x.NormalizedBarcode == normalized && x.IsActive)
            .WhereIf(input.ExpectedTargetType.HasValue, x => x.BarcodeTargetType == input.ExpectedTargetType!.Value);

        var identities = await AsyncExecuter.ToListAsync(identityQuery);

        if (identities.Count > 0)
        {
            foreach (var ident in identities)
            {
                var candidate = await BuildCandidateAsync(ident);
                if (candidate != null)
                {
                    result.Candidates.Add(candidate);
                }
            }

            var primary = identities.First();
            await PopulateResultFromIdentityAsync(result, primary);
            result.IsFound = true;
            return result;
        }

        // 2. Fallback: Direct Legacy Product Barcode Check
        var productQuery = (await _productRepository.GetQueryableAsync())
            .Where(x => x.Barcode == normalized && x.IsActive);
        var legacyProduct = await AsyncExecuter.FirstOrDefaultAsync(productQuery);

        if (legacyProduct != null)
        {
            var variantQuery = (await _variantRepository.GetQueryableAsync())
                .Where(v => v.ProductId == legacyProduct.Id);

            var variants = await AsyncExecuter.ToListAsync(variantQuery);
            var defVariant = variants.FirstOrDefault(v => v.IsDefault) ?? variants.FirstOrDefault();

            result.IsFound = true;
            result.TargetType = BarcodeTargetType.ProductVariant;
            result.TargetId = defVariant?.Id ?? legacyProduct.Id;
            result.ProductId = legacyProduct.Id;
            result.ProductName = legacyProduct.Name;
            result.ProductVariantId = defVariant?.Id;
            result.Sku = defVariant?.VariantSku ?? legacyProduct.Sku;
            result.VariantName = defVariant?.VariantName;
            return result;
        }

        // 3. Fallback: Direct Location Barcode Check
        var locQuery = (await _locationRepository.GetQueryableAsync())
            .Where(x => x.Barcode == normalized && x.IsActive);
        var location = await AsyncExecuter.FirstOrDefaultAsync(locQuery);

        if (location != null)
        {
            result.IsFound = true;
            result.TargetType = BarcodeTargetType.WarehouseLocation;
            result.TargetId = location.Id;
            result.LocationId = location.Id;
            result.LocationBarcode = location.Barcode;
            result.LocationDisplayName = $"{location.Aisle}-{location.Rack}-{location.Shelf}-{location.Bin}";
            return result;
        }

        return result;
    }

    private async Task PopulateResultFromIdentityAsync(BarcodeLookupResultDto result, BarcodeIdentity identity)
    {
        result.TargetType = identity.BarcodeTargetType;
        result.TargetId = identity.ProductVariantId
                          ?? identity.LicensePlateId
                          ?? identity.WarehouseLocationId
                          ?? identity.InventoryLotId
                          ?? identity.InventorySerialId;

        switch (identity.BarcodeTargetType)
        {
            case BarcodeTargetType.ProductVariant when identity.ProductVariantId.HasValue:
                var variant = await _variantRepository.FindAsync(identity.ProductVariantId.Value);
                if (variant != null)
                {
                    result.ProductVariantId = variant.Id;
                    result.Sku = variant.VariantSku;
                    result.VariantName = variant.VariantName;
                    result.ProductId = variant.ProductId;

                    var product = await _productRepository.FindAsync(variant.ProductId);
                    if (product != null)
                    {
                        result.ProductName = product.Name;
                    }

                    var pbcQuery = (await _productBarcodeRepository.GetQueryableAsync())
                        .Where(x => x.BarcodeIdentityId == identity.Id);
                    var pbc = await AsyncExecuter.FirstOrDefaultAsync(pbcQuery);

                    if (pbc != null)
                    {
                        result.PackagingLevel = pbc.PackagingLevel;
                        result.UnitsPerPackage = 1;
                    }

                    // Check if associated with a Supplier Barcode Mapping
                    var smQuery = (await _supplierMappingRepository.GetQueryableAsync())
                        .Where(x => x.BarcodeIdentityId == identity.Id && x.IsActive);
                    var sm = await AsyncExecuter.FirstOrDefaultAsync(smQuery);
                    if (sm != null)
                    {
                        result.IsSupplierBarcode = true;
                        result.SupplierId = sm.SupplierId;
                        result.SupplierProductCode = sm.SupplierPartNumber;

                        var supplier = await _supplierRepository.FindAsync(sm.SupplierId);
                        result.SupplierName = supplier?.Name;
                    }
                }
                break;

            case BarcodeTargetType.LicensePlate when identity.LicensePlateId.HasValue:
                var lp = await _licensePlateRepository.FindAsync(identity.LicensePlateId.Value);
                if (lp != null)
                {
                    result.LicensePlateId = lp.Id;
                    result.LicensePlateNumber = lp.LpnCode;
                }
                break;

            case BarcodeTargetType.WarehouseLocation when identity.WarehouseLocationId.HasValue:
                var loc = await _locationRepository.FindAsync(identity.WarehouseLocationId.Value);
                if (loc != null)
                {
                    result.LocationId = loc.Id;
                    result.LocationBarcode = loc.Barcode;
                    result.LocationDisplayName = $"{loc.Aisle}-{loc.Rack}-{loc.Shelf}-{loc.Bin}";
                }
                break;

            case BarcodeTargetType.InventoryLot when identity.InventoryLotId.HasValue:
                var lot = await _lotRepository.FindAsync(identity.InventoryLotId.Value);
                if (lot != null)
                {
                    result.LotId = lot.Id;
                    result.LotBatchNumber = lot.LotNumber;
                    result.ExpirationDate = lot.ExpiryDate;
                    result.ProductId = lot.ProductId;
                    result.ProductVariantId = lot.ProductVariantId;

                    var prod = await _productRepository.FindAsync(lot.ProductId);
                    result.ProductName = prod?.Name;
                }
                break;

            case BarcodeTargetType.InventorySerial when identity.InventorySerialId.HasValue:
                var ser = await _serialRepository.FindAsync(identity.InventorySerialId.Value);
                if (ser != null)
                {
                    result.SerialId = ser.Id;
                    result.SerialNumber = ser.SerialNumber;
                    result.ProductId = ser.ProductId;
                    result.ProductVariantId = ser.ProductVariantId;

                    var prod = await _productRepository.FindAsync(ser.ProductId);
                    result.ProductName = prod?.Name;
                }
                break;
        }
    }

    private async Task<BarcodeLookupCandidateDto?> BuildCandidateAsync(BarcodeIdentity identity)
    {
        switch (identity.BarcodeTargetType)
        {
            case BarcodeTargetType.ProductVariant when identity.ProductVariantId.HasValue:
                var v = await _variantRepository.FindAsync(identity.ProductVariantId.Value);
                if (v == null) return null;
                var p = await _productRepository.FindAsync(v.ProductId);
                return new BarcodeLookupCandidateDto
                {
                    TargetType = identity.BarcodeTargetType,
                    TargetId = v.Id,
                    Identifier = v.VariantSku,
                    DisplayText = $"{p?.Name} ({v.VariantSku})",
                    ProductId = v.ProductId,
                    ProductVariantId = v.Id
                };

            case BarcodeTargetType.LicensePlate when identity.LicensePlateId.HasValue:
                var lp = await _licensePlateRepository.FindAsync(identity.LicensePlateId.Value);
                if (lp == null) return null;
                return new BarcodeLookupCandidateDto
                {
                    TargetType = identity.BarcodeTargetType,
                    TargetId = lp.Id,
                    Identifier = lp.LpnCode,
                    DisplayText = $"LPN: {lp.LpnCode} ({lp.Type})"
                };

            case BarcodeTargetType.WarehouseLocation when identity.WarehouseLocationId.HasValue:
                var loc = await _locationRepository.FindAsync(identity.WarehouseLocationId.Value);
                if (loc == null) return null;
                return new BarcodeLookupCandidateDto
                {
                    TargetType = identity.BarcodeTargetType,
                    TargetId = loc.Id,
                    Identifier = loc.Barcode,
                    DisplayText = $"Loc: {loc.Aisle}-{loc.Rack}-{loc.Shelf}-{loc.Bin}"
                };

            case BarcodeTargetType.InventoryLot when identity.InventoryLotId.HasValue:
                var lot = await _lotRepository.FindAsync(identity.InventoryLotId.Value);
                if (lot == null) return null;
                return new BarcodeLookupCandidateDto
                {
                    TargetType = identity.BarcodeTargetType,
                    TargetId = lot.Id,
                    Identifier = lot.LotNumber,
                    DisplayText = $"Lot: {lot.LotNumber}"
                };

            case BarcodeTargetType.InventorySerial when identity.InventorySerialId.HasValue:
                var ser = await _serialRepository.FindAsync(identity.InventorySerialId.Value);
                if (ser == null) return null;
                return new BarcodeLookupCandidateDto
                {
                    TargetType = identity.BarcodeTargetType,
                    TargetId = ser.Id,
                    Identifier = ser.SerialNumber,
                    DisplayText = $"Serial: {ser.SerialNumber}"
                };

            default:
                return null;
        }
    }
}
