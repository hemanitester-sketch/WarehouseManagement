import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

export interface BarcodeLookupInput {
  barcodeValue: string;
  expectedTargetType?: number | null;
  warehouseId?: string | null;
}

export interface BarcodeLookupCandidateDto {
  targetType: number;
  targetId: string;
  identifier: string;
  displayText: string;
  productId?: string | null;
  productVariantId?: string | null;
}

export interface BarcodeLookupResultDto {
  isFound: boolean;
  scannedValue: string;
  normalizedValue: string;
  symbology: number;
  targetType?: number | null;
  targetId?: string | null;
  productId?: string | null;
  productVariantId?: string | null;
  sku?: string | null;
  productName?: string | null;
  variantName?: string | null;
  packagingLevel?: number | null;
  unitsPerPackage?: number | null;
  licensePlateId?: string | null;
  licensePlateNumber?: string | null;
  locationId?: string | null;
  locationBarcode?: string | null;
  locationDisplayName?: string | null;
  lotId?: string | null;
  lotBatchNumber?: string | null;
  expirationDate?: string | null;
  serialId?: string | null;
  serialNumber?: string | null;
  isSupplierBarcode: boolean;
  supplierId?: string | null;
  supplierName?: string | null;
  supplierProductCode?: string | null;
  hasMultipleMatches: boolean;
  candidates: BarcodeLookupCandidateDto[];
}

@Injectable({
  providedIn: 'root',
})
export class BarcodeLookupService {
  private restService = inject(RestService);
  apiName = 'Default';

  resolveBarcode = (input: BarcodeLookupInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BarcodeLookupResultDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/barcode-lookup/resolve-barcode',
      body: input,
    },
    { apiName: this.apiName, ...config });
}

