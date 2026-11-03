using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Barcodes;
using WarehouseManagement.Inventory;

namespace WarehouseManagement.Products;

public class ProductVariantAppService : ApplicationService, IProductVariantAppService
{
    private readonly IRepository<ProductVariant, Guid> _variantRepository;
    private readonly IReadOnlyRepository<Product, Guid> _productRepository;
    private readonly IReadOnlyRepository<ProductAttribute, Guid> _attributeRepository;
    private readonly IRepository<BarcodeIdentity, Guid> _barcodeIdentityRepository;
    private readonly IRepository<ProductBarcode, Guid> _productBarcodeRepository;
    private readonly IReadOnlyRepository<InventoryItem, Guid> _inventoryItemRepository;
    private readonly IBarcodeGenerator _barcodeGenerator;

    public ProductVariantAppService(
        IRepository<ProductVariant, Guid> variantRepository,
        IReadOnlyRepository<Product, Guid> productRepository,
        IReadOnlyRepository<ProductAttribute, Guid> attributeRepository,
        IRepository<BarcodeIdentity, Guid> barcodeIdentityRepository,
        IRepository<ProductBarcode, Guid> productBarcodeRepository,
        IReadOnlyRepository<InventoryItem, Guid> inventoryItemRepository,
        IBarcodeGenerator barcodeGenerator)
    {
        _variantRepository = variantRepository;
        _productRepository = productRepository;
        _attributeRepository = attributeRepository;
        _barcodeIdentityRepository = barcodeIdentityRepository;
        _productBarcodeRepository = productBarcodeRepository;
        _inventoryItemRepository = inventoryItemRepository;
        _barcodeGenerator = barcodeGenerator;
    }

    public async Task<ProductVariantDto> GetAsync(Guid id)
    {
        var query = (await _variantRepository.WithDetailsAsync(
                x => x.Product,
                x => x.AttributeValues,
                x => x.Barcodes))
            .Where(x => x.Id == id);

        var variant = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (variant == null)
        {
            throw new UserFriendlyException("Product variant not found.");
        }

        return await MapToDtoAsync(variant);
    }

    public async Task<List<ProductVariantDto>> GetListByProductIdAsync(Guid productId)
    {
        var query = (await _variantRepository.WithDetailsAsync(
                x => x.Product,
                x => x.AttributeValues,
                x => x.Barcodes))
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.VariantSku);

        var variants = await AsyncExecuter.ToListAsync(query);
        var dtos = new List<ProductVariantDto>();
        foreach (var v in variants)
        {
            dtos.Add(await MapToDtoAsync(v));
        }
        return dtos;
    }

    public async Task<ProductVariantDto> CreateAsync(CreateProductVariantDto input)
    {
        var product = await _productRepository.GetAsync(input.ProductId);

        var combinationHash = CalculateCombinationHash(input.Attributes);

        var existingVariantsQuery = (await _variantRepository.GetQueryableAsync())
            .Where(x => x.ProductId == input.ProductId);
        var existingVariants = await AsyncExecuter.ToListAsync(existingVariantsQuery);

        if (existingVariants.Any(x => x.CombinationHash == combinationHash))
        {
            throw new UserFriendlyException("A variant with this exact combination of attributes already exists for this product.");
        }

        var sku = !string.IsNullOrWhiteSpace(input.Sku)
            ? input.Sku.Trim().ToUpperInvariant()
            : GenerateVariantSku(product.Sku, input.Attributes);

        if (existingVariants.Any(x => x.VariantSku == sku))
        {
            throw new UserFriendlyException($"A variant with SKU '{sku}' already exists.");
        }

        var variantName = !string.IsNullOrWhiteSpace(input.VariantName)
            ? input.VariantName.Trim()
            : GenerateVariantName(product.Name, input.Attributes);

        var variant = new ProductVariant(
            GuidGenerator.Create(),
            input.ProductId,
            sku,
            variantName,
            combinationHash,
            unitPrice: input.SellingPrice > 0 ? input.SellingPrice : product.UnitPrice,
            purchaseCost: input.PurchasePrice ?? 0,
            weightKg: input.WeightKg > 0 ? input.WeightKg : product.WeightKg,
            lengthCm: input.LengthCm ?? product.LengthCm,
            widthCm: input.WidthCm ?? product.WidthCm,
            heightCm: input.HeightCm ?? product.HeightCm,
            imageUrl: input.ImageUrl?.Trim(),
            isDefault: input.IsDefault || existingVariants.Count == 0,
            isActive: true,
            tenantId: CurrentTenant.Id
        );

        foreach (var attr in input.Attributes)
        {
            variant.AttributeValues.Add(new ProductVariantAttributeValue(
                GuidGenerator.Create(),
                variant.Id,
                attr.AttributeId,
                attr.ValueId,
                attr.ValueText.Trim(),
                CurrentTenant.Id
            ));
        }

        await _variantRepository.InsertAsync(variant, autoSave: true);

        var barcodeValue = input.PrimaryBarcode?.Trim();
        if (string.IsNullOrWhiteSpace(barcodeValue))
        {
            barcodeValue = await _barcodeGenerator.GenerateInternalCode128Async(BarcodeTargetType.ProductVariant, CurrentTenant.Id);
        }

        await AddBarcodeInternalAsync(variant, barcodeValue, isPrimary: true);

        return await GetAsync(variant.Id);
    }

    public async Task<ProductVariantDto> UpdateAsync(Guid id, UpdateProductVariantDto input)
    {
        var variant = await _variantRepository.GetAsync(id);

        var normalizedSku = input.Sku.Trim().ToUpperInvariant();
        if (variant.VariantSku != normalizedSku)
        {
            var existsQuery = (await _variantRepository.GetQueryableAsync())
                .Where(x => x.VariantSku == normalizedSku && x.Id != id);
            var exists = await AsyncExecuter.AnyAsync(existsQuery);

            if (exists)
            {
                throw new UserFriendlyException($"A variant with SKU '{normalizedSku}' already exists.");
            }
            variant.SetVariantSku(normalizedSku);
        }

        if (!string.IsNullOrWhiteSpace(input.VariantName))
        {
            variant.SetVariantName(input.VariantName.Trim());
        }

        variant.PurchaseCost = input.PurchasePrice ?? 0;
        variant.UnitPrice = input.SellingPrice;
        variant.WeightKg = input.WeightKg;
        variant.LengthCm = input.LengthCm;
        variant.WidthCm = input.WidthCm;
        variant.HeightCm = input.HeightCm;
        variant.IsActive = input.IsActive;

        if (input.IsDefault && !variant.IsDefault)
        {
            var othersQuery = (await _variantRepository.GetQueryableAsync())
                .Where(x => x.ProductId == variant.ProductId && x.Id != id);
            var others = await AsyncExecuter.ToListAsync(othersQuery);

            foreach (var other in others)
            {
                other.IsDefault = false;
            }
            variant.IsDefault = true;
        }

        await _variantRepository.UpdateAsync(variant, autoSave: true);
        return await GetAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var hasStockQuery = (await _inventoryItemRepository.GetQueryableAsync())
            .Where(x => x.ProductVariantId == id && x.QuantityOnHand > 0);
        var hasStock = await AsyncExecuter.AnyAsync(hasStockQuery);

        if (hasStock)
        {
            throw new UserFriendlyException("Cannot delete variant because active inventory exists for it.");
        }

        await _variantRepository.DeleteAsync(id);
    }

    public async Task<List<GeneratedVariantPreviewDto>> GenerateVariantMatrixPreviewAsync(GenerateVariantMatrixInput input)
    {
        var product = await _productRepository.GetAsync(input.ProductId);
        var activeOptions = input.Options.Where(o => o.SelectedValues.Count > 0).ToList();

        if (activeOptions.Count == 0)
        {
            return new List<GeneratedVariantPreviewDto>();
        }

        var combinations = CartesianProduct(activeOptions);
        var previews = new List<GeneratedVariantPreviewDto>();

        foreach (var combo in combinations)
        {
            var attrs = new List<VariantAttributeSelectionDto>();
            for (var i = 0; i < activeOptions.Count; i++)
            {
                attrs.Add(new VariantAttributeSelectionDto
                {
                    AttributeId = activeOptions[i].AttributeId,
                    ValueText = combo[i]
                });
            }

            var hash = CalculateCombinationHash(attrs);
            var sku = GenerateVariantSku(product.Sku, attrs);
            var name = GenerateVariantName(product.Name, attrs);

            previews.Add(new GeneratedVariantPreviewDto
            {
                GeneratedSku = sku,
                GeneratedVariantName = name,
                CombinationHash = hash,
                Attributes = attrs
            });
        }

        return previews;
    }

    public async Task<List<ProductVariantDto>> BatchCreateVariantsAsync(Guid productId, List<CreateProductVariantDto> variants)
    {
        var created = new List<ProductVariantDto>();
        foreach (var v in variants)
        {
            v.ProductId = productId;
            var res = await CreateAsync(v);
            created.Add(res);
        }
        return created;
    }

    public async Task<ProductBarcodeDto> AddBarcodeAsync(Guid variantId, string barcodeValue, BarcodeSymbology symbology, bool isPrimary = false)
    {
        var variant = await _variantRepository.GetAsync(variantId);
        var productBarcode = await AddBarcodeInternalAsync(variant, barcodeValue, isPrimary, symbology);

        return new ProductBarcodeDto
        {
            Id = productBarcode.Id,
            BarcodeValue = barcodeValue,
            Symbology = symbology,
            BarcodeKind = BarcodeKind.InternalCode128,
            PackagingLevel = productBarcode.PackagingLevel,
            UnitsPerPackage = 1,
            IsPrimary = productBarcode.IsPrimary,
            IsActive = true
        };
    }

    public async Task RemoveBarcodeAsync(Guid barcodeId)
    {
        var pb = await _productBarcodeRepository.GetAsync(barcodeId);
        if (pb.IsPrimary)
        {
            throw new UserFriendlyException("Cannot remove the primary barcode of a variant.");
        }

        await _productBarcodeRepository.DeleteAsync(pb);
    }

    private async Task<ProductBarcode> AddBarcodeInternalAsync(
        ProductVariant variant,
        string barcodeValue,
        bool isPrimary = false,
        BarcodeSymbology? symbologyOverride = null)
    {
        var normalized = _barcodeGenerator.NormalizeBarcode(barcodeValue);
        var symbology = symbologyOverride ?? _barcodeGenerator.DetectSymbology(normalized);

        var identQuery = (await _barcodeIdentityRepository.GetQueryableAsync())
            .Where(x => x.NormalizedBarcode == normalized);
        var identity = await AsyncExecuter.FirstOrDefaultAsync(identQuery);

        if (identity != null)
        {
            if (identity.BarcodeTargetType != BarcodeTargetType.ProductVariant || identity.ProductVariantId != variant.Id)
            {
                throw new UserFriendlyException($"Barcode '{normalized}' is already registered for target '{identity.BarcodeTargetType}'.");
            }
        }
        else
        {
            identity = new BarcodeIdentity(
                GuidGenerator.Create(),
                normalized,
                BarcodeTargetType.ProductVariant,
                BarcodeKind.InternalCode128,
                symbology,
                productVariantId: variant.Id,
                tenantId: CurrentTenant.Id
            );
            await _barcodeIdentityRepository.InsertAsync(identity, autoSave: true);
        }

        var pbQuery = (await _productBarcodeRepository.GetQueryableAsync())
            .Where(x => x.ProductVariantId == variant.Id && x.BarcodeIdentityId == identity.Id);
        var existingPb = await AsyncExecuter.FirstOrDefaultAsync(pbQuery);

        if (existingPb != null)
        {
            if (isPrimary && !existingPb.IsPrimary)
            {
                await ClearPrimaryBarcodeFlagAsync(variant.Id);
                existingPb.IsPrimary = true;
                await _productBarcodeRepository.UpdateAsync(existingPb, autoSave: true);
            }
            return existingPb;
        }

        if (isPrimary)
        {
            await ClearPrimaryBarcodeFlagAsync(variant.Id);
        }

        var pb = new ProductBarcode(
            GuidGenerator.Create(),
            identity.Id,
            variant.ProductId,
            variant.Id,
            BarcodePackagingLevel.Each,
            isPrimary,
            LabelPrintFormat.Zpl_4x6,
            CurrentTenant.Id
        );

        await _productBarcodeRepository.InsertAsync(pb, autoSave: true);
        return pb;
    }

    private async Task ClearPrimaryBarcodeFlagAsync(Guid variantId)
    {
        var primariesQuery = (await _productBarcodeRepository.GetQueryableAsync())
            .Where(x => x.ProductVariantId == variantId && x.IsPrimary);
        var primaries = await AsyncExecuter.ToListAsync(primariesQuery);

        foreach (var p in primaries)
        {
            p.IsPrimary = false;
        }
    }

    private static string CalculateCombinationHash(List<VariantAttributeSelectionDto> attributes)
    {
        if (attributes == null || attributes.Count == 0)
        {
            return "DEFAULT";
        }

        var ordered = attributes
            .OrderBy(x => x.AttributeId)
            .Select(x => $"{x.AttributeId:N}:{x.ValueText.Trim().ToUpperInvariant()}")
            .ToList();

        var canonicalString = string.Join("|", ordered);
        var bytes = Encoding.UTF8.GetBytes(canonicalString);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static string GenerateVariantSku(string parentSku, List<VariantAttributeSelectionDto> attributes)
    {
        if (attributes.Count == 0)
        {
            return $"{parentSku}-DEF";
        }

        var parts = attributes
            .OrderBy(a => a.AttributeId)
            .Select(a => SanitizeForSku(a.ValueText))
            .ToList();

        return $"{parentSku}-{string.Join("-", parts)}".ToUpperInvariant();
    }

    private static string GenerateVariantName(string parentName, List<VariantAttributeSelectionDto> attributes)
    {
        if (attributes.Count == 0)
        {
            return parentName;
        }

        var parts = attributes.Select(a => a.ValueText).ToList();
        return $"{parentName} ({string.Join(", ", parts)})";
    }

    private static string SanitizeForSku(string value)
    {
        var sb = new StringBuilder();
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }
        return sb.Length > 0 ? sb.ToString() : "VAR";
    }

    private static List<List<string>> CartesianProduct(List<AttributeMatrixOptionInput> options)
    {
        var result = new List<List<string>> { new() };

        foreach (var opt in options)
        {
            var temp = new List<List<string>>();
            foreach (var existing in result)
            {
                foreach (var val in opt.SelectedValues)
                {
                    var newList = new List<string>(existing) { val };
                    temp.Add(newList);
                }
            }
            result = temp;
        }

        return result;
    }

    private async Task<ProductVariantDto> MapToDtoAsync(ProductVariant v)
    {
        var barcodeIds = v.Barcodes?.Select(b => b.BarcodeIdentityId).ToList() ?? new List<Guid>();
        var identMap = new Dictionary<Guid, BarcodeIdentity>();
        if (barcodeIds.Count > 0)
        {
            var idQuery = (await _barcodeIdentityRepository.GetQueryableAsync())
                .Where(x => barcodeIds.Contains(x.Id));
            var idList = await AsyncExecuter.ToListAsync(idQuery);
            identMap = idList.ToDictionary(x => x.Id);
        }

        var attrIds = v.AttributeValues?.Select(a => a.ProductAttributeId).ToList() ?? new List<Guid>();
        var attrMap = new Dictionary<Guid, ProductAttribute>();
        if (attrIds.Count > 0)
        {
            var aQuery = (await _attributeRepository.GetQueryableAsync())
                .Where(x => attrIds.Contains(x.Id));
            var aList = await AsyncExecuter.ToListAsync(aQuery);
            attrMap = aList.ToDictionary(x => x.Id);
        }

        var primaryPb = v.Barcodes?.FirstOrDefault(b => b.IsPrimary);
        BarcodeIdentity? primaryIdent = null;
        if (primaryPb != null) identMap.TryGetValue(primaryPb.BarcodeIdentityId, out primaryIdent);

        return new ProductVariantDto
        {
            Id = v.Id,
            ProductId = v.ProductId,
            ProductSku = v.Product?.Sku ?? string.Empty,
            ProductName = v.Product?.Name ?? string.Empty,
            Sku = v.VariantSku,
            VariantName = v.VariantName,
            CombinationHash = v.CombinationHash,
            PurchasePrice = v.PurchaseCost,
            SellingPrice = v.UnitPrice,
            WeightKg = v.WeightKg,
            VolumeCm3 = 0,
            LengthCm = v.LengthCm,
            WidthCm = v.WidthCm,
            HeightCm = v.HeightCm,
            IsActive = v.IsActive,
            IsDefault = v.IsDefault,
            ImageUrl = v.ImageUrl,
            PrimaryBarcode = primaryIdent?.NormalizedBarcode,
            CreationTime = v.CreationTime,
            CreatorId = v.CreatorId,
            Attributes = v.AttributeValues?
                .Select(a =>
                {
                    attrMap.TryGetValue(a.ProductAttributeId, out var attr);
                    return new ProductVariantAttributeValueDto
                    {
                        AttributeId = a.ProductAttributeId,
                        AttributeName = attr?.Name ?? string.Empty,
                        AttributeCode = attr?.Code ?? string.Empty,
                        ValueId = a.ProductAttributeValueId,
                        ValueText = a.CustomValue ?? string.Empty
                    };
                }).ToList() ?? new List<ProductVariantAttributeValueDto>(),
            Barcodes = v.Barcodes?
                .Select(b =>
                {
                    identMap.TryGetValue(b.BarcodeIdentityId, out var ident);
                    return new ProductBarcodeDto
                    {
                        Id = b.Id,
                        BarcodeValue = ident?.NormalizedBarcode ?? string.Empty,
                        Symbology = ident?.Symbology ?? BarcodeSymbology.Code128,
                        BarcodeKind = BarcodeKind.InternalCode128,
                        PackagingLevel = b.PackagingLevel,
                        UnitsPerPackage = 1,
                        IsPrimary = b.IsPrimary,
                        IsActive = true
                    };
                }).ToList() ?? new List<ProductBarcodeDto>()
        };
    }
}
