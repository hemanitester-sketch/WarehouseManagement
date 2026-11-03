using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Products;
using WarehouseManagement.Suppliers;

namespace WarehouseManagement.Barcodes;

public class SupplierBarcodeAppService : ApplicationService, ISupplierBarcodeAppService
{
    private readonly IRepository<SupplierBarcodeMapping, Guid> _mappingRepository;
    private readonly IRepository<BarcodeIdentity, Guid> _barcodeIdentityRepository;
    private readonly IReadOnlyRepository<ProductVariant, Guid> _variantRepository;
    private readonly IReadOnlyRepository<Product, Guid> _productRepository;
    private readonly IReadOnlyRepository<Supplier, Guid> _supplierRepository;
    private readonly IBarcodeGenerator _barcodeGenerator;

    public SupplierBarcodeAppService(
        IRepository<SupplierBarcodeMapping, Guid> mappingRepository,
        IRepository<BarcodeIdentity, Guid> barcodeIdentityRepository,
        IReadOnlyRepository<ProductVariant, Guid> variantRepository,
        IReadOnlyRepository<Product, Guid> productRepository,
        IReadOnlyRepository<Supplier, Guid> supplierRepository,
        IBarcodeGenerator barcodeGenerator)
    {
        _mappingRepository = mappingRepository;
        _barcodeIdentityRepository = barcodeIdentityRepository;
        _variantRepository = variantRepository;
        _productRepository = productRepository;
        _supplierRepository = supplierRepository;
        _barcodeGenerator = barcodeGenerator;
    }

    public async Task<SupplierBarcodeMappingDto> CreateAsync(CreateSupplierBarcodeMappingDto input)
    {
        var normalizedBarcode = _barcodeGenerator.NormalizeBarcode(input.SupplierBarcode);
        if (string.IsNullOrWhiteSpace(normalizedBarcode))
        {
            throw new UserFriendlyException("Supplier barcode cannot be empty.");
        }

        var variant = await _variantRepository.GetAsync(input.ProductVariantId);
        var product = await _productRepository.GetAsync(variant.ProductId);
        var supplier = await _supplierRepository.GetAsync(input.SupplierId);

        // 1. Check or create BarcodeIdentity
        var identQuery = (await _barcodeIdentityRepository.GetQueryableAsync())
            .Where(x => x.NormalizedBarcode == normalizedBarcode);
        var identity = await AsyncExecuter.FirstOrDefaultAsync(identQuery);

        if (identity != null)
        {
            if (identity.BarcodeTargetType != BarcodeTargetType.ProductVariant ||
                (identity.ProductVariantId.HasValue && identity.ProductVariantId.Value != input.ProductVariantId))
            {
                var conflictVariant = identity.ProductVariantId.HasValue
                    ? await _variantRepository.FindAsync(identity.ProductVariantId.Value)
                    : null;

                throw new UserFriendlyException(
                    $"The barcode '{normalizedBarcode}' is already assigned to variant '{conflictVariant?.VariantSku ?? identity.ProductVariantId?.ToString()}'. " +
                    "An identical barcode cannot resolve to different product variants.");
            }
        }
        else
        {
            var symbology = _barcodeGenerator.DetectSymbology(normalizedBarcode);
            identity = new BarcodeIdentity(
                GuidGenerator.Create(),
                normalizedBarcode,
                BarcodeTargetType.ProductVariant,
                BarcodeKind.InternalCode128,
                symbology,
                productVariantId: variant.Id,
                tenantId: CurrentTenant.Id
            );
            await _barcodeIdentityRepository.InsertAsync(identity, autoSave: true);
        }

        // 2. Check existing mappings for this barcode identity
        var mappingQuery = (await _mappingRepository.GetQueryableAsync())
            .Where(x => x.BarcodeIdentityId == identity.Id);
        var existingMappings = await AsyncExecuter.ToListAsync(mappingQuery);

        var crossVariantConflict = existingMappings.FirstOrDefault(x => x.ProductVariantId != input.ProductVariantId);
        if (crossVariantConflict != null)
        {
            var conflictVariant = await _variantRepository.FindAsync(crossVariantConflict.ProductVariantId);
            throw new UserFriendlyException(
                $"The barcode '{normalizedBarcode}' is already mapped to variant '{conflictVariant?.VariantSku ?? crossVariantConflict.ProductVariantId.ToString()}'. " +
                "An identical barcode cannot resolve to different product variants.");
        }

        var sameSupplierMapping = existingMappings.FirstOrDefault(x => x.SupplierId == input.SupplierId);
        if (sameSupplierMapping != null)
        {
            if (sameSupplierMapping.IsActive)
            {
                throw new UserFriendlyException($"Supplier '{supplier.Name}' already has an active mapping for barcode '{normalizedBarcode}'.");
            }

            sameSupplierMapping.IsActive = true;
            sameSupplierMapping.SupplierPartNumber = input.SupplierProductCode;
            await _mappingRepository.UpdateAsync(sameSupplierMapping, autoSave: true);

            return MapToDto(sameSupplierMapping, supplier, variant, product, normalizedBarcode);
        }

        // 3. Create Supplier Mapping
        var newMapping = new SupplierBarcodeMapping(
            GuidGenerator.Create(),
            input.SupplierId,
            identity.Id,
            input.ProductVariantId,
            input.SupplierProductCode,
            isActive: true,
            tenantId: CurrentTenant.Id
        );

        await _mappingRepository.InsertAsync(newMapping, autoSave: true);

        return MapToDto(newMapping, supplier, variant, product, normalizedBarcode);
    }

    public async Task<PagedResultDto<SupplierBarcodeMappingDto>> GetListAsync(GetSupplierBarcodeMappingsDto input)
    {
        var query = await _mappingRepository.GetQueryableAsync();

        query = query
            .WhereIf(input.SupplierId.HasValue, x => x.SupplierId == input.SupplierId!.Value)
            .WhereIf(input.ProductVariantId.HasValue, x => x.ProductVariantId == input.ProductVariantId!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.SupplierPartNumber != null && x.SupplierPartNumber.Contains(input.Filter!));

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime desc" : input.Sorting;
        var pagedQuery = query.OrderBy(sorting).PageBy(input.SkipCount, input.MaxResultCount);
        var items = await AsyncExecuter.ToListAsync(pagedQuery);

        if (items.Count == 0)
        {
            return new PagedResultDto<SupplierBarcodeMappingDto>(totalCount, new List<SupplierBarcodeMappingDto>());
        }

        var supplierIds = items.Select(x => x.SupplierId).Distinct().ToList();
        var variantIds = items.Select(x => x.ProductVariantId).Distinct().ToList();
        var identityIds = items.Select(x => x.BarcodeIdentityId).Distinct().ToList();

        var suppliersQuery = (await _supplierRepository.GetQueryableAsync())
            .Where(s => supplierIds.Contains(s.Id));
        var suppliers = (await AsyncExecuter.ToListAsync(suppliersQuery)).ToDictionary(s => s.Id);

        var variantsQuery = (await _variantRepository.GetQueryableAsync())
            .Where(v => variantIds.Contains(v.Id));
        var variants = (await AsyncExecuter.ToListAsync(variantsQuery)).ToDictionary(v => v.Id);

        var identitiesQuery = (await _barcodeIdentityRepository.GetQueryableAsync())
            .Where(i => identityIds.Contains(i.Id));
        var identities = (await AsyncExecuter.ToListAsync(identitiesQuery)).ToDictionary(i => i.Id);

        var productIds = variants.Values.Select(v => v.ProductId).Distinct().ToList();
        var productsQuery = (await _productRepository.GetQueryableAsync())
            .Where(p => productIds.Contains(p.Id));
        var products = (await AsyncExecuter.ToListAsync(productsQuery)).ToDictionary(p => p.Id);

        var dtos = items.Select(m =>
        {
            suppliers.TryGetValue(m.SupplierId, out var s);
            variants.TryGetValue(m.ProductVariantId, out var v);
            identities.TryGetValue(m.BarcodeIdentityId, out var ident);
            Product? p = null;
            if (v != null) products.TryGetValue(v.ProductId, out p);

            return new SupplierBarcodeMappingDto
            {
                Id = m.Id,
                SupplierId = m.SupplierId,
                SupplierName = s?.Name ?? "Unknown Supplier",
                ProductVariantId = m.ProductVariantId,
                VariantSku = v?.VariantSku ?? "N/A",
                VariantName = v?.VariantName ?? v?.VariantSku ?? "N/A",
                ProductId = v?.ProductId ?? Guid.Empty,
                ProductName = p?.Name ?? "N/A",
                SupplierBarcode = ident?.NormalizedBarcode ?? string.Empty,
                SupplierProductCode = m.SupplierPartNumber,
                PackagingLevel = BarcodePackagingLevel.Each,
                UnitsPerPackage = 1,
                IsActive = m.IsActive,
                CreationTime = m.CreationTime,
                CreatorId = m.CreatorId
            };
        }).ToList();

        return new PagedResultDto<SupplierBarcodeMappingDto>(totalCount, dtos);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _mappingRepository.DeleteAsync(id);
    }

    private static SupplierBarcodeMappingDto MapToDto(
        SupplierBarcodeMapping mapping,
        Supplier supplier,
        ProductVariant variant,
        Product product,
        string barcode)
    {
        return new SupplierBarcodeMappingDto
        {
            Id = mapping.Id,
            SupplierId = mapping.SupplierId,
            SupplierName = supplier.Name,
            ProductVariantId = mapping.ProductVariantId,
            VariantSku = variant.VariantSku,
            VariantName = variant.VariantName ?? variant.VariantSku,
            ProductId = product.Id,
            ProductName = product.Name,
            SupplierBarcode = barcode,
            SupplierProductCode = mapping.SupplierPartNumber,
            PackagingLevel = BarcodePackagingLevel.Each,
            UnitsPerPackage = 1,
            IsActive = mapping.IsActive,
            CreationTime = mapping.CreationTime,
            CreatorId = mapping.CreatorId
        };
    }
}
