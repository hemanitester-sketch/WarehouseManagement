import type { CreateUpdateInboundOrderDto, GetInboundOrderListDto, InboundOrderDto, ReceiveInboundGoodsDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InboundOrderService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  
  create = (input: CreateUpdateInboundOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InboundOrderDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/inbound-order',
      body: input,
    },
      { apiName: this.apiName,...config });
  

  
  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/inbound-order/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InboundOrderDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/inbound-order/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  getList = (input: GetInboundOrderListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InboundOrderDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/inbound-order',
      params: { filter: input.filter, warehouseId: input.warehouseId, status: input.status, expectedDateFrom: input.expectedDateFrom, expectedDateTo: input.expectedDateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
      { apiName: this.apiName,...config });
  

  
  receiveGoods = (input: ReceiveInboundGoodsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InboundOrderDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/inbound-order/receive-goods',
      body: input,
    },
      { apiName: this.apiName,...config });
  

  
  update = (id: string, input: CreateUpdateInboundOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InboundOrderDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/inbound-order/${id}`,
      body: input,
    },
      { apiName: this.apiName,...config });
}
