using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using WarehouseManagement.Barcodes;

namespace WarehouseManagement.Products;

public interface IProductVariantAppService : IApplicationService
{
    Task<ProductVariantDto> GetAsync(Guid id);

    Task<List<ProductVariantDto>> GetListByProductIdAsync(Guid productId);

    Task<ProductVariantDto> CreateAsync(CreateProductVariantDto input);

    Task<ProductVariantDto> UpdateAsync(Guid id, UpdateProductVariantDto input);

    Task DeleteAsync(Guid id);

    Task<List<GeneratedVariantPreviewDto>> GenerateVariantMatrixPreviewAsync(GenerateVariantMatrixInput input);

    Task<List<ProductVariantDto>> BatchCreateVariantsAsync(Guid productId, List<CreateProductVariantDto> variants);

    Task<ProductBarcodeDto> AddBarcodeAsync(Guid variantId, string barcodeValue, BarcodeSymbology symbology, bool isPrimary = false);

    Task RemoveBarcodeAsync(Guid barcodeId);
}
