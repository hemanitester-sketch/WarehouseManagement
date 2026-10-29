import type {
  CreateUpdateFacultyMemberDto,
  FacultyMemberDto,
  FacultyMemberLookupDto,
  GetFacultyMemberListDto,
} from './models';
import { RestService, Rest, PagedResultDto, ListResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FacultyMemberService {
  private restService = inject(RestService);
  apiName = 'Default';

  create = (input: CreateUpdateFacultyMemberDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FacultyMemberDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: '/api/app/faculty-member',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/app/faculty-member/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FacultyMemberDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: `/api/app/faculty-member/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  getList = (input: GetFacultyMemberListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FacultyMemberDto>>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/faculty-member',
        params: {
          filter: input.filter,
          warehouseId: input.warehouseId,
          designation: input.designation,
          shift: input.shift,
          status: input.status,
          isActive: input.isActive,
          sorting: input.sorting,
          skipCount: input.skipCount,
          maxResultCount: input.maxResultCount,
        },
      },
      { apiName: this.apiName, ...config }
    );

  update = (id: string, input: CreateUpdateFacultyMemberDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FacultyMemberDto>(
      {
        method: 'PUT',
        headers: { Accept: 'application/json' },
        url: `/api/app/faculty-member/${id}`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  getLookup = (warehouseId?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<FacultyMemberLookupDto>>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/faculty-member/lookup',
        params: { warehouseId },
      },
      { apiName: this.apiName, ...config }
    );

  findByCode = (code: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FacultyMemberDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: `/api/app/faculty-member/by-code/${code}`,
      },
      { apiName: this.apiName, ...config }
    );
}

