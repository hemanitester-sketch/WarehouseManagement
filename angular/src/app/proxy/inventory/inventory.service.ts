import type { AdjustStockDto, GetInventoryListDto, GetInventoryTransactionListDto, InventoryItemDto, InventorySummaryDto, InventoryTransactionDto, TransferStockDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InventoryService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  
  adjustStock = (input: AdjustStockDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryItemDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/inventory/adjust-stock',
      body: input,
    },
      { apiName: this.apiName,...config });
  

  
  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryItemDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/inventory/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  getList = (input: GetInventoryListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryItemDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/inventory',
      params: { filter: input.filter, warehouseId: input.warehouseId, zoneId: input.zoneId, locationId: input.locationId, productId: input.productId, lotBatchNumber: input.lotBatchNumber, serialNumber: input.serialNumber, lpn: input.lpn, status: input.status, expiredOnly: input.expiredOnly, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
      { apiName: this.apiName,...config });
  

  
  getStockSummary = (warehouseId: string, productId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventorySummaryDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/inventory/stock-summary',
      params: { warehouseId, productId },
    },
      { apiName: this.apiName,...config });
  

  
  getTransactions = (input: GetInventoryTransactionListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryTransactionDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/inventory/transactions',
      params: { warehouseId: input.warehouseId, productId: input.productId, transactionType: input.transactionType, referenceNumber: input.referenceNumber, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
      { apiName: this.apiName,...config });
  

  
  transferStock = (input: TransferStockDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryItemDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/inventory/transfer-stock',
      body: input,
    },
      { apiName: this.apiName,...config });
}
