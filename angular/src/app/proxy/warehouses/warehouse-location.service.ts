import type { CreateUpdateWarehouseLocationDto, GetWarehouseLocationListDto, WarehouseLocationDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WarehouseLocationService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  
  create = (input: CreateUpdateWarehouseLocationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseLocationDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/warehouse-location',
      body: input,
    },
      { apiName: this.apiName,...config });
  

  
  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/warehouse-location/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  findByBarcode = (barcode: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseLocationDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/warehouse-location/find-by-barcode',
      params: { barcode },
    },
      { apiName: this.apiName,...config });
  

  
  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseLocationDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/warehouse-location/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  getList = (input: GetWarehouseLocationListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WarehouseLocationDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/warehouse-location',
      params: { filter: input.filter, warehouseId: input.warehouseId, zoneId: input.zoneId, locationType: input.locationType, status: input.status, isPickFace: input.isPickFace, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
      { apiName: this.apiName,...config });
  

  
  update = (id: string, input: CreateUpdateWarehouseLocationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseLocationDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/warehouse-location/${id}`,
      body: input,
    },
      { apiName: this.apiName,...config });
}
