import type {
  CreateUpdateSupplierDto,
  GetSupplierListDto,
  SupplierDto,
  SupplierLookupDto,
} from './models';
import { RestService, Rest, PagedResultDto, ListResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SupplierService {
  private restService = inject(RestService);
  apiName = 'Default';

  create = (input: CreateUpdateSupplierDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SupplierDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: '/api/app/supplier',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/app/supplier/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SupplierDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: `/api/app/supplier/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  getList = (input: GetSupplierListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SupplierDto>>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/supplier',
        params: {
          filter: input.filter,
          city: input.city,
          paymentTerms: input.paymentTerms,
          isActive: input.isActive,
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config }
    );

  update = (id: string, input: CreateUpdateSupplierDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SupplierDto>(
      {
        method: 'PUT',
        headers: { Accept: 'application/json' },
        url: `/api/app/supplier/${id}`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  getLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<SupplierLookupDto>>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/supplier/lookup',
      },
      { apiName: this.apiName, ...config }
    );
}

