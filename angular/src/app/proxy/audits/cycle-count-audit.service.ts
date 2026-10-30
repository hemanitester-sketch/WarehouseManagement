import type {
  ApproveReconcileAuditDto,
  CreateCycleCountAuditDto,
  CreateCycleCountAuditItemDto,
  CycleCountAuditDto,
  CycleCountDashboardMetricsDto,
  CycleCountType,
  GetCycleCountListDto,
  RecordCountItemDto,
  SubmitCountDto,
} from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CycleCountAuditService {
  private restService = inject(RestService);
  apiName = 'Default';

  create = (input: CreateCycleCountAuditDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CycleCountAuditDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: '/api/app/cycle-count-audit',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CycleCountAuditDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: `/api/app/cycle-count-audit/${id}`,
      },
      { apiName: this.apiName, ...config }
    );

  getList = (input: GetCycleCountListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CycleCountAuditDto>>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/cycle-count-audit',
        params: {
          filter: input.filter,
          warehouseId: input.warehouseId,
          zoneId: input.zoneId,
          countType: input.countType,
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

  startCounting = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CycleCountAuditDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/cycle-count-audit/${id}/start-counting`,
      },
      { apiName: this.apiName, ...config }
    );

  recordCounts = (id: string, items: RecordCountItemDto[], config?: Partial<Rest.Config>) =>
    this.restService.request<any, CycleCountAuditDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/cycle-count-audit/${id}/record-counts`,
        body: items,
      },
      { apiName: this.apiName, ...config }
    );

  submitForReview = (id: string, input?: SubmitCountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CycleCountAuditDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/cycle-count-audit/${id}/submit-for-review`,
        body: input || {},
      },
      { apiName: this.apiName, ...config }
    );

  requestReCount = (id: string, reason?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CycleCountAuditDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/cycle-count-audit/${id}/request-re-count`,
        params: { reason },
      },
      { apiName: this.apiName, ...config }
    );

  approveAndReconcile = (id: string, input?: ApproveReconcileAuditDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CycleCountAuditDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/cycle-count-audit/${id}/approve-and-reconcile`,
        body: input || {},
      },
      { apiName: this.apiName, ...config }
    );

  cancelAudit = (id: string, reason?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CycleCountAuditDto>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: `/api/app/cycle-count-audit/${id}/cancel-audit`,
        params: { reason },
      },
      { apiName: this.apiName, ...config }
    );

  getDashboardMetrics = (warehouseId?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CycleCountDashboardMetricsDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/cycle-count-audit/dashboard-metrics',
        params: { warehouseId },
      },
      { apiName: this.apiName, ...config }
    );

  generateSnapshotItems = (
    warehouseId: string,
    countType: CycleCountType,
    zoneId?: string,
    specificProductId?: string,
    config?: Partial<Rest.Config>
  ) =>
    this.restService.request<any, CreateCycleCountAuditItemDto[]>(
      {
        method: 'POST',
        headers: { Accept: 'application/json' },
        url: '/api/app/cycle-count-audit/generate-snapshot-items',
        params: {
          warehouseId,
          countType,
          zoneId,
          specificProductId,
        },
      },
      { apiName: this.apiName, ...config }
    );
}

