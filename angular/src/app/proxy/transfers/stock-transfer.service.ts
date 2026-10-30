import type { CreateUpdateStockTransferDto, DispatchTransferDto, GetStockTransferListDto, StockTransferDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StockTransferService {
  private restService = inject(RestService);
  apiName = 'Default';

  create = (input: CreateUpdateStockTransferDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: '/api/app/stock-transfer',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  update = (id: string, input: CreateUpdateStockTransferDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>(
      {
        method: 'PUT',
        headers: { Accept: 'application/json' },
        url: `/api/app/stock-transfer/${id}`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/app/stock-transfer/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: `/api/app/stock-transfer/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  getList = (input: GetStockTransferListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StockTransferDto>>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/stock-transfer',
        params: {
          filter: input.filter,
          type: input.type,
          status: input.status,
          sourceWarehouseId: input.sourceWarehouseId,
          destinationWarehouseId: input.destinationWarehouseId,
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config }
    );

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>(
      {
        method: 'POST',
        url: `/api/app/stock-transfer/${id}/approve`,
      },
      { apiName: this.apiName, ...config }
    );

  dispatch = (id: string, input?: DispatchTransferDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>(
      {
        method: 'POST',
        url: `/api/app/stock-transfer/${id}/dispatch`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  complete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>(
      {
        method: 'POST',
        url: `/api/app/stock-transfer/${id}/complete`,
      },
      { apiName: this.apiName, ...config }
    );

  cancel = (id: string, reason?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransferDto>(
      {
        method: 'POST',
        url: `/api/app/stock-transfer/${id}/cancel`,
        params: { reason },
      },
      { apiName: this.apiName, ...config }
    );
}
