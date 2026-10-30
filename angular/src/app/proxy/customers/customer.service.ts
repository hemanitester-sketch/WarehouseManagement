import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto, ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type {
  CustomerDto,
  CreateUpdateCustomerDto,
  GetCustomerListDto,
  CustomerLookupDto,
  CustomerDashboardMetricsDto,
  CustomerStatus
} from './models';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  apiName = 'Default';

  create = (input: CreateUpdateCustomerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDto>({
      method: 'POST',
      url: '/api/app/customer',
      body: input,
    },
    { apiName: this.apiName, ...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/customer/${id}`,
    },
    { apiName: this.apiName, ...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDto>({
      method: 'GET',
      url: `/api/app/customer/${id}`,
    },
    { apiName: this.apiName, ...config });

  getList = (input: GetCustomerListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CustomerDto>>({
      method: 'GET',
      url: '/api/app/customer',
      params: {
        filter: input.filter,
        customerType: input.customerType,
        status: input.status,
        city: input.city,
        paymentTerms: input.paymentTerms,
        isActive: input.isActive,
        sorting: input.sorting,
        skipCount: input.skipCount,
        maxResultCount: input.maxResultCount,
      },
    },
    { apiName: this.apiName, ...config });

  update = (id: string, input: CreateUpdateCustomerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDto>({
      method: 'PUT',
      url: `/api/app/customer/${id}`,
      body: input,
    },
    { apiName: this.apiName, ...config });

  getLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<CustomerLookupDto>>({
      method: 'GET',
      url: '/api/app/customer/lookup',
    },
    { apiName: this.apiName, ...config });

  getDashboardMetrics = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDashboardMetricsDto>({
      method: 'GET',
      url: '/api/app/customer/dashboard-metrics',
    },
    { apiName: this.apiName, ...config });

  toggleStatus = (id: string, newStatus: CustomerStatus, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDto>({
      method: 'POST',
      url: `/api/app/customer/${id}/toggle-status`,
      params: { newStatus },
    },
    { apiName: this.apiName, ...config });

  constructor(private restService: RestService) {}
}

