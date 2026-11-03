import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type {
  ProductVariantDto,
  CreateProductVariantDto,
  UpdateProductVariantDto,
  GenerateVariantMatrixInput,
  GeneratedVariantPreviewDto,
  ProductBarcodeDto,
  BarcodeSymbology,
} from './models';

@Injectable({
  providedIn: 'root',
})
export class ProductVariantService {
  private restService = inject(RestService);
  apiName = 'Default';

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductVariantDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/product-variant/${id}`,
    },
    { apiName: this.apiName, ...config });

  getListByProductId = (productId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductVariantDto[]>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/product-variant/by-product-id/${productId}`,
    },
    { apiName: this.apiName, ...config });

  create = (input: CreateProductVariantDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductVariantDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/product-variant',
      body: input,
    },
    { apiName: this.apiName, ...config });

  update = (id: string, input: UpdateProductVariantDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductVariantDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/product-variant/${id}`,
      body: input,
    },
    { apiName: this.apiName, ...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/product-variant/${id}`,
    },
    { apiName: this.apiName, ...config });

  generateVariantMatrixPreview = (input: GenerateVariantMatrixInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, GeneratedVariantPreviewDto[]>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/product-variant/generate-variant-matrix-preview',
      body: input,
    },
    { apiName: this.apiName, ...config });

  batchCreateVariants = (productId: string, variants: CreateProductVariantDto[], config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductVariantDto[]>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: `/api/app/product-variant/batch-create-variants/${productId}`,
      body: variants,
    },
    { apiName: this.apiName, ...config });

  addBarcode = (variantId: string, barcodeValue: string, symbology: BarcodeSymbology, isPrimary: boolean = false, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductBarcodeDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: `/api/app/product-variant/barcode/${variantId}`,
      params: { barcodeValue, symbology, isPrimary },
    },
    { apiName: this.apiName, ...config });

  removeBarcode = (barcodeId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/product-variant/barcode/${barcodeId}`,
    },
    { apiName: this.apiName, ...config });
}
