import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type {
  ProductAttributeDto,
  CreateUpdateProductAttributeDto,
  CategoryAttributeTemplateDto,
  AssignCategoryAttributeDto,
} from './models';

@Injectable({
  providedIn: 'root',
})
export class ProductAttributeService {
  private restService = inject(RestService);
  apiName = 'Default';

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductAttributeDto[]>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/product-attribute',
    },
    { apiName: this.apiName, ...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductAttributeDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/product-attribute/${id}`,
    },
    { apiName: this.apiName, ...config });

  create = (input: CreateUpdateProductAttributeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductAttributeDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/product-attribute',
      body: input,
    },
    { apiName: this.apiName, ...config });

  update = (id: string, input: CreateUpdateProductAttributeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductAttributeDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/product-attribute/${id}`,
      body: input,
    },
    { apiName: this.apiName, ...config });

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/product-attribute/${id}`,
    },
    { apiName: this.apiName, ...config });

  getAttributesByCategoryId = (categoryId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CategoryAttributeTemplateDto[]>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/product-attribute/attributes-by-category-id/${categoryId}`,
    },
    { apiName: this.apiName, ...config });

  assignAttributeToCategory = (input: AssignCategoryAttributeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/product-attribute/assign-attribute-to-category',
      body: input,
    },
    { apiName: this.apiName, ...config });

  removeAttributeFromCategory = (categoryId: string, attributeId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/product-attribute/attribute-from-category?categoryId=${categoryId}&attributeId=${attributeId}`,
    },
    { apiName: this.apiName, ...config });
}
