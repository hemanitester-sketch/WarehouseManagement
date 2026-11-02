import type {
  ExecutiveWarehouseKpisDto,
  GetWarehouseReportFilterDto,
  InboundSupplierSlaReportItemDto,
  OutboundFulfillmentReportItemDto,
  QualityReverseLogisticsReportDto,
  SpaceUtilizationReportItemDto,
  StockAgingReportItemDto,
} from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WarehouseReportService {
  private restService = inject(RestService);
  apiName = 'Default';

  getExecutiveKpis = (input: GetWarehouseReportFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExecutiveWarehouseKpisDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/warehouse-report/executive-kpis',
        params: {
          warehouseId: input.warehouseId,
          fromDate: input.fromDate,
          toDate: input.toDate,
          filter: input.filter,
        },
      },
      { apiName: this.apiName, ...config }
    );

  getStockAgingReport = (input: GetWarehouseReportFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockAgingReportItemDto[]>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/warehouse-report/stock-aging-report',
        params: {
          warehouseId: input.warehouseId,
          fromDate: input.fromDate,
          toDate: input.toDate,
          filter: input.filter,
        },
      },
      { apiName: this.apiName, ...config }
    );

  getSpaceUtilizationReport = (input: GetWarehouseReportFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SpaceUtilizationReportItemDto[]>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/warehouse-report/space-utilization-report',
        params: {
          warehouseId: input.warehouseId,
          fromDate: input.fromDate,
          toDate: input.toDate,
          filter: input.filter,
        },
      },
      { apiName: this.apiName, ...config }
    );

  getSupplierSlaReport = (input: GetWarehouseReportFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InboundSupplierSlaReportItemDto[]>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/warehouse-report/supplier-sla-report',
        params: {
          warehouseId: input.warehouseId,
          fromDate: input.fromDate,
          toDate: input.toDate,
          filter: input.filter,
        },
      },
      { apiName: this.apiName, ...config }
    );

  getOutboundFulfillmentReport = (input: GetWarehouseReportFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OutboundFulfillmentReportItemDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/warehouse-report/outbound-fulfillment-report',
        params: {
          warehouseId: input.warehouseId,
          fromDate: input.fromDate,
          toDate: input.toDate,
          filter: input.filter,
        },
      },
      { apiName: this.apiName, ...config }
    );

  getQualityAndReturnsReport = (input: GetWarehouseReportFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, QualityReverseLogisticsReportDto>(
      {
        method: 'GET',
        headers: { Accept: 'application/json' },
        url: '/api/app/warehouse-report/quality-and-returns-report',
        params: {
          warehouseId: input.warehouseId,
          fromDate: input.fromDate,
          toDate: input.toDate,
          filter: input.filter,
        },
      },
      { apiName: this.apiName, ...config }
    );
}
