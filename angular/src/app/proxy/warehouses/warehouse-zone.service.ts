import type { CreateUpdateWarehouseZoneDto, GetWarehouseZoneListDto, WarehouseZoneDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WarehouseZoneService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  
  create = (input: CreateUpdateWarehouseZoneDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseZoneDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/warehouse-zone',
      body: input,
    },
      { apiName: this.apiName,...config });
  

  
  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/warehouse-zone/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseZoneDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/warehouse-zone/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  getList = (input: GetWarehouseZoneListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WarehouseZoneDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/warehouse-zone',
      params: { filter: input.filter, warehouseId: input.warehouseId, storageType: input.storageType, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
      { apiName: this.apiName,...config });
  

  
  update = (id: string, input: CreateUpdateWarehouseZoneDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseZoneDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/warehouse-zone/${id}`,
      body: input,
    },
      { apiName: this.apiName,...config });
}
