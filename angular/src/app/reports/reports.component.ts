import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WarehouseReportService } from '../proxy/reports/warehouse-report.service';
import {
  ExecutiveWarehouseKpisDto,
  GetWarehouseReportFilterDto,
  InboundSupplierSlaReportItemDto,
  OutboundFulfillmentReportItemDto,
  QualityReverseLogisticsReportDto,
  SpaceUtilizationReportItemDto,
  StockAgingReportItemDto,
} from '../proxy/reports/models';
import { WarehouseService } from '../proxy/warehouses/warehouse.service';
import { WarehouseDto } from '../proxy/warehouses/models';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss'],
})
export class ReportsComponent implements OnInit {
  private reportService = inject(WarehouseReportService);
  private warehouseService = inject(WarehouseService);

  activeTab: 'executive' | 'aging' | 'space' | 'inbound' | 'outbound' | 'quality' = 'executive';
  isLoading = false;
  searchQuery = '';
  selectedWarehouseId = '';
  selectedDatePreset: 'all' | 'today' | '7d' | '30d' | '90d' = '30d';

  warehouses: WarehouseDto[] = [];

  kpis: ExecutiveWarehouseKpisDto = {
    totalInventoryValuation: 0,
    totalStockUnits: 0,
    totalActiveSkus: 0,
    lowStockSkuCount: 0,
    overallSpaceUtilizationRate: 0,
    totalOccupiedBins: 0,
    totalWarehouseLocations: 0,
    inboundThroughput30Days: 0,
    outboundThroughput30Days: 0,
    auditAccuracyRate: 100,
    returnRatePercentage: 0,
    pendingInboundOrders: 0,
    pendingOutboundOrders: 0,
    totalSalvageOrScrapValue: 0,
  };

  agingList: StockAgingReportItemDto[] = [];
  filteredAgingList: StockAgingReportItemDto[] = [];

  spaceList: SpaceUtilizationReportItemDto[] = [];
  filteredSpaceList: SpaceUtilizationReportItemDto[] = [];

  supplierList: InboundSupplierSlaReportItemDto[] = [];
  filteredSupplierList: InboundSupplierSlaReportItemDto[] = [];

  outboundReport: OutboundFulfillmentReportItemDto | null = null;
  qualityReport: QualityReverseLogisticsReportDto | null = null;

  reportGeneratedDate: Date = new Date();

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadActiveTabData();
  }

  loadWarehouses(): void {
    this.warehouseService.getList({ maxResultCount: 100 }).subscribe({
      next: res => {
        this.warehouses = res.items || [];
      },
      error: err => console.error('Error fetching facilities', err),
    });
  }

  getFilterDto(): GetWarehouseReportFilterDto {
    const dto: GetWarehouseReportFilterDto = {};
    if (this.selectedWarehouseId) {
      dto.warehouseId = this.selectedWarehouseId;
    }

    const now = new Date();
    if (this.selectedDatePreset === 'today') {
      const startOfDay = new Date(now.getFullYear(), now.getMonth(), now.getDate());
      dto.fromDate = startOfDay.toISOString();
      dto.toDate = now.toISOString();
    } else if (this.selectedDatePreset === '7d') {
      const d7 = new Date();
      d7.setDate(now.getDate() - 7);
      dto.fromDate = d7.toISOString();
      dto.toDate = now.toISOString();
    } else if (this.selectedDatePreset === '30d') {
      const d30 = new Date();
      d30.setDate(now.getDate() - 30);
      dto.fromDate = d30.toISOString();
      dto.toDate = now.toISOString();
    } else if (this.selectedDatePreset === '90d') {
      const d90 = new Date();
      d90.setDate(now.getDate() - 90);
      dto.fromDate = d90.toISOString();
      dto.toDate = now.toISOString();
    }

    if (this.searchQuery && this.searchQuery.trim()) {
      dto.filter = this.searchQuery.trim();
    }

    return dto;
  }

  loadActiveTabData(): void {
    this.isLoading = true;
    this.reportGeneratedDate = new Date();
    const filter = this.getFilterDto();

    // Always fetch executive KPIs for the top summary cards
    this.reportService.getExecutiveKpis(filter).subscribe({
      next: res => {
        this.kpis = res || this.kpis;
      },
      error: err => console.error('Failed to load KPIs', err),
    });

    switch (this.activeTab) {
      case 'executive':
        // Overview also fetches aging & space for snapshot charts
        this.reportService.getStockAgingReport(filter).subscribe({
          next: list => {
            this.agingList = list || [];
            this.applyLocalFilters();
            this.isLoading = false;
          },
          error: () => (this.isLoading = false),
        });
        break;

      case 'aging':
        this.reportService.getStockAgingReport(filter).subscribe({
          next: list => {
            this.agingList = list || [];
            this.applyLocalFilters();
            this.isLoading = false;
          },
          error: () => (this.isLoading = false),
        });
        break;

      case 'space':
        this.reportService.getSpaceUtilizationReport(filter).subscribe({
          next: list => {
            this.spaceList = list || [];
            this.applyLocalFilters();
            this.isLoading = false;
          },
          error: () => (this.isLoading = false),
        });
        break;

      case 'inbound':
        this.reportService.getSupplierSlaReport(filter).subscribe({
          next: list => {
            this.supplierList = list || [];
            this.applyLocalFilters();
            this.isLoading = false;
          },
          error: () => (this.isLoading = false),
        });
        break;

      case 'outbound':
        this.reportService.getOutboundFulfillmentReport(filter).subscribe({
          next: res => {
            this.outboundReport = res;
            this.isLoading = false;
          },
          error: () => (this.isLoading = false),
        });
        break;

      case 'quality':
        this.reportService.getQualityAndReturnsReport(filter).subscribe({
          next: res => {
            this.qualityReport = res;
            this.isLoading = false;
          },
          error: () => (this.isLoading = false),
        });
        break;
    }
  }

  setTab(tab: 'executive' | 'aging' | 'space' | 'inbound' | 'outbound' | 'quality'): void {
    this.activeTab = tab;
    this.loadActiveTabData();
  }

  setDatePreset(preset: 'all' | 'today' | '7d' | '30d' | '90d'): void {
    this.selectedDatePreset = preset;
    this.loadActiveTabData();
  }

  onWarehouseChange(): void {
    this.loadActiveTabData();
  }

  onSearch(): void {
    this.applyLocalFilters();
  }

  applyLocalFilters(): void {
    const q = this.searchQuery ? this.searchQuery.trim().toLowerCase() : '';
    if (!q) {
      this.filteredAgingList = [...this.agingList];
      this.filteredSpaceList = [...this.spaceList];
      this.filteredSupplierList = [...this.supplierList];
      return;
    }

    this.filteredAgingList = this.agingList.filter(
      x =>
        x.sku.toLowerCase().includes(q) ||
        x.productName.toLowerCase().includes(q) ||
        x.categoryName.toLowerCase().includes(q) ||
        x.warehouseName.toLowerCase().includes(q) ||
        x.ageBracket.toLowerCase().includes(q) ||
        x.fsnClassification.toLowerCase().includes(q)
    );

    this.filteredSpaceList = this.spaceList.filter(
      x =>
        x.warehouseCode.toLowerCase().includes(q) ||
        x.warehouseName.toLowerCase().includes(q) ||
        (x.city && x.city.toLowerCase().includes(q))
    );

    this.filteredSupplierList = this.supplierList.filter(
      x =>
        x.supplierName.toLowerCase().includes(q) ||
        x.supplierCode.toLowerCase().includes(q)
    );
  }

  getAgingBracketCount(bracketPrefix: string): number {
    return this.agingList.filter(x => x.ageBracket.startsWith(bracketPrefix)).length;
  }

  getAgingBracketValuation(bracketPrefix: string): number {
    return this.agingList
      .filter(x => x.ageBracket.startsWith(bracketPrefix))
      .reduce((sum, item) => sum + item.totalValuation, 0);
  }

  getFsnCount(classificationPrefix: string): number {
    return this.agingList.filter(x => x.fsnClassification.startsWith(classificationPrefix)).length;
  }

  exportToCsv(): void {
    let csvContent = 'data:text/csv;charset=utf-8,';
    const dateStr = new Date().toISOString().split('T')[0];
    let fileName = `WareFlow_Report_${this.activeTab}_${dateStr}.csv`;

    switch (this.activeTab) {
      case 'aging':
      case 'executive':
        csvContent += 'SKU,Product Name,Category,Warehouse,On-Hand Qty,Unit Price (INR),Valuation (INR),Age (Days),Age Bracket,FSN Velocity,90-Day Shipped Units\n';
        this.filteredAgingList.forEach(item => {
          const row = [
            `"${item.sku}"`,
            `"${item.productName.replace(/"/g, '""')}"`,
            `"${item.categoryName}"`,
            `"${item.warehouseName}"`,
            item.quantityOnHand,
            item.unitPrice,
            item.totalValuation,
            item.ageInDays,
            `"${item.ageBracket}"`,
            `"${item.fsnClassification}"`,
            item.outboundShippedLast90Days,
          ].join(',');
          csvContent += row + '\n';
        });
        break;

      case 'space':
        csvContent += 'Facility Code,Facility Name,City,Total Zones,Total Bins,Occupied Bins,Available Bins,Space Utilization %,Total Stock Units\n';
        this.filteredSpaceList.forEach(item => {
          const row = [
            `"${item.warehouseCode}"`,
            `"${item.warehouseName.replace(/"/g, '""')}"`,
            `"${item.city || ''}"`,
            item.totalZones,
            item.totalBins,
            item.occupiedBins,
            item.availableBins,
            `${item.utilizationPercentage}%`,
            item.totalStockUnitsStored,
          ].join(',');
          csvContent += row + '\n';
        });
        break;

      case 'inbound':
        csvContent += 'Supplier Code,Supplier Name,Total POs,Completed POs,Pending POs,Total Expected Qty,Total Received Qty,Fulfillment %,On-Time Delivery %\n';
        this.filteredSupplierList.forEach(item => {
          const row = [
            `"${item.supplierCode}"`,
            `"${item.supplierName.replace(/"/g, '""')}"`,
            item.totalPurchaseOrders,
            item.completedOrders,
            item.pendingOrders,
            item.totalExpectedQuantity,
            item.totalReceivedQuantity,
            `${item.fulfillmentRatePercentage}%`,
            `${item.onTimeDeliveryPercentage}%`,
          ].join(',');
          csvContent += row + '\n';
        });
        break;

      case 'outbound':
        csvContent += 'Customer Code,Customer Name,Total Orders,Units Delivered,Total Sales Value (INR)\n';
        if (this.outboundReport?.topCustomers) {
          this.outboundReport.topCustomers.forEach(cust => {
            const row = [
              `"${cust.customerCode}"`,
              `"${cust.customerName.replace(/"/g, '""')}"`,
              cust.totalOrders,
              cust.totalUnitsDelivered,
              cust.totalOrderValue,
            ].join(',');
            csvContent += row + '\n';
          });
        }
        break;

      case 'quality':
        csvContent += 'Return Reason,Claim Count,Percentage Share\n';
        if (this.qualityReport?.topReturnReasons) {
          this.qualityReport.topReturnReasons.forEach(r => {
            csvContent += `"${r.reason}",${r.count},${r.percentage}%\n`;
          });
        }
        break;
    }

    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  printReport(): void {
    window.print();
  }
}

