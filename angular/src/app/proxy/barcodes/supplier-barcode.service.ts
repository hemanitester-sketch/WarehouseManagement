import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto, PagedAndSortedResultRequestDto, FullAuditedEntityDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

export interface SupplierBarcodeMappingDto extends FullAuditedEntityDto<string> {
  supplierId: string;
  supplierName: string;
  productId: string;
  productName: string;
  productVariantId: string;
  variantSku: string;
  variantName: string;
  supplierBarcode: string;
  supplierProductCode?: string | null;
  packagingLevel: number;
  unitsPerPackage: number;
  isActive: boolean;
}

export interface CreateSupplierBarcodeMappingDto {
  supplierId: string;
  productVariantId: string;
  supplierBarcode: string;
  supplierProductCode?: string | null;
  packagingLevel: number;
  unitsPerPackage: number;
}

export interface GetSupplierBarcodeMappingsDto extends PagedAndSortedResultRequestDto {
  supplierId?: string | null;
  productId?: string | null;
  productVariantId?: string | null;
  filter?: string | null;
  isActive?: boolean | null;
}

@Injectable({
  providedIn: 'root',
})
export class SupplierBarcodeService {
  private restService = inject(RestService);
  apiName = 'Default';

  create = (input: CreateSupplierBarcodeMappingDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SupplierBarcodeMappingDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/supplier-barcode',
      body: input,
    },
    { apiName: this.apiName, ...config });

  getList = (input: GetSupplierBarcodeMappingsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SupplierBarcodeMappingDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/supplier-barcode',
      params: {
        supplierId: input.supplierId,
        productId: input.productId,
        productVariantId: input.productVariantId,
        filter: input.filter,
        isActive: input.isActive,
        sorting: input.sorting,
        skipCount: input.skipCount,
        maxResultCount: input.maxResultCount,
      },
    },
    { apiName: this.apiName, ...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/supplier-barcode/${id}`,
    },
    { apiName: this.apiName, ...config });
}

