import type { CreateUpdateProductDto, GetProductListDto, ProductDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  
  create = (input: CreateUpdateProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/product',
      body: input,
    },
      { apiName: this.apiName,...config });
  

  
  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/product/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  findByBarcode = (barcode: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/product/find-by-barcode',
      params: { barcode },
    },
      { apiName: this.apiName,...config });
  

  
  findBySku = (sku: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/product/find-by-sku',
      params: { sku },
    },
      { apiName: this.apiName,...config });
  

  
  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/product/${id}`,
    },
      { apiName: this.apiName,...config });
  

  
  getList = (input: GetProductListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProductDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/product',
      params: { filter: input.filter, category: input.category, uom: input.uom, requiredStorageType: input.requiredStorageType, isLotTracked: input.isLotTracked, isExpiryTracked: input.isExpiryTracked, isSerialTracked: input.isSerialTracked, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
      { apiName: this.apiName,...config });
  

  
  update = (id: string, input: CreateUpdateProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/product/${id}`,
      body: input,
    },
      { apiName: this.apiName,...config });
}
