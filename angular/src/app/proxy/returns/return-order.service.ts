import type {
  CreateReturnOrderDto,
  GetReturnOrderListDto,
  InspectReturnOrderDto,
  ReceiveReturnDto,
  ReturnDashboardMetricsDto,
  ReturnOrderDto,
} from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ReturnOrderService {
  private restService = inject(RestService);
  apiName = 'Default';

  create = (input: CreateReturnOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReturnOrderDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: '/api/app/return-order',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReturnOrderDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: `/api/app/return-order/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  getList = (input: GetReturnOrderListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ReturnOrderDto>>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/return-order',
        params: {
          filter: input.filter,
          warehouseId: input.warehouseId,
          customerId: input.customerId,
          supplierId: input.supplierId,
          returnType: input.returnType,
          status: input.status,
          fromDate: input.fromDate,
          toDate: input.toDate,
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config }
    );

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReturnOrderDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/return-order/${id}/approve`,
      },
      { apiName: this.apiName, ...config }
    );

  markInTransit = (
    id: string,
    params?: { carrierName?: string; trackingNumber?: string },
    config?: Partial<Rest.Config>
  ) =>
    this.restService.request<any, ReturnOrderDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/return-order/${id}/mark-in-transit`,
        params: { carrierName: params?.carrierName, trackingNumber: params?.trackingNumber },
      },
      { apiName: this.apiName, ...config }
    );

  receive = (id: string, input: ReceiveReturnDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReturnOrderDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/return-order/${id}/receive`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  inspectAndResolve = (id: string, input: InspectReturnOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReturnOrderDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/return-order/${id}/inspect-and-resolve`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  reject = (id: string, reason?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReturnOrderDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/return-order/${id}/reject`,
        params: { reason },
      },
      { apiName: this.apiName, ...config }
    );

  cancel = (id: string, reason?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReturnOrderDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/return-order/${id}/cancel`,
        params: { reason },
      },
      { apiName: this.apiName, ...config }
    );

  getDashboardMetrics = (warehouseId?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReturnDashboardMetricsDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/return-order/dashboard-metrics',
        params: { warehouseId },
      },
      { apiName: this.apiName, ...config }
    );
}
