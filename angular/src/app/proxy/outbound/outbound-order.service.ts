import type { AllocateOrderStockDto, CreateUpdateOutboundOrderDto, GetOutboundOrderListDto, OutboundOrderDto, PickOrderGoodsDto, ShipOrderDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class OutboundOrderService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  
  allocateStock = (input: AllocateOrderStockDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OutboundOrderDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/outbound-order/allocate-stock',
      body: input,
    },
      { apiName: this.apiName,...config });
  

  
  create = (input: CreateUpdateOutboundOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OutboundOrderDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/outbound-order',
      body: input,
    },
      { apiName: this.apiName,...config });
  

  
  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/outbound-order/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OutboundOrderDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/outbound-order/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  getList = (input: GetOutboundOrderListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<OutboundOrderDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/outbound-order',
      params: { filter: input.filter, warehouseId: input.warehouseId, status: input.status, priority: input.priority, orderDateFrom: input.orderDateFrom, orderDateTo: input.orderDateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
      { apiName: this.apiName,...config });
  

  
  pickGoods = (input: PickOrderGoodsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OutboundOrderDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/outbound-order/pick-goods',
      body: input,
    },
      { apiName: this.apiName,...config });
  

  
  shipOrder = (input: ShipOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OutboundOrderDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/outbound-order/ship-order',
      body: input,
    },
      { apiName: this.apiName,...config });
  

  
  update = (id: string, input: CreateUpdateOutboundOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OutboundOrderDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/outbound-order/${id}`,
      body: input,
    },
      { apiName: this.apiName,...config });
}
