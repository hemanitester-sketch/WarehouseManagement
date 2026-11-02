import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import {
  ReturnOrderService,
  ReturnOrderDto,
  ReturnOrderItemDto,
  CreateReturnOrderDto,
  CreateReturnOrderItemDto,
  ReturnOrderStatus,
  ReturnType,
  ReturnReason,
  ItemConditionGrade,
  ItemResolutionAction,
  ReturnDashboardMetricsDto,
  ReceiveReturnDto,
  InspectReturnOrderDto,
} from '../proxy/returns';
import { WarehouseService, WarehouseDto } from '../proxy/warehouses';
import { WarehouseLocationService, WarehouseLocationDto } from '../proxy/warehouses';
import { ProductService, ProductDto } from '../proxy/products';
import { CustomerService, CustomerDto } from '../proxy/customers';
import { SupplierService, SupplierDto } from '../proxy/suppliers';

@Component({
  selector: 'app-returns',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './returns.component.html',
  styleUrls: ['./returns.component.scss'],
})
export class ReturnsComponent implements OnInit {
  private returnService = inject(ReturnOrderService);
  private warehouseService = inject(WarehouseService);
  private locationService = inject(WarehouseLocationService);
  private productService = inject(ProductService);
  private customerService = inject(CustomerService);
  private supplierService = inject(SupplierService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  // Enums for template
  ReturnOrderStatus = ReturnOrderStatus;
  ReturnType = ReturnType;
  ReturnReason = ReturnReason;
  ItemConditionGrade = ItemConditionGrade;
  ItemResolutionAction = ItemResolutionAction;

  // Master Data
  returns: ReturnOrderDto[] = [];
  warehouses: WarehouseDto[] = [];
  locations: WarehouseLocationDto[] = [];
  products: ProductDto[] = [];
  customers: CustomerDto[] = [];
  suppliers: SupplierDto[] = [];

  // Metrics
  metrics: ReturnDashboardMetricsDto = {
    totalReturns: 0,
    pendingApprovalCount: 0,
    inTransitCount: 0,
    awaitingInspectionCount: 0,
    completedCount: 0,
    totalRestockedUnits: 0,
    totalScrappedUnits: 0,
    totalRefundValue: 0,
    recoveryRatePercentage: 100,
  };

  // State
  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedTypeFilter: ReturnType | '' = '';
  selectedStatusTab: 'ALL' | 'PENDING' | 'IN_TRANSIT' | 'RECEIVED' | 'COMPLETED' = 'ALL';
  selectedWarehouseFilter = '';
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  // Create Modal / Drawer State
  isCreateModalOpen = false;
  newReturnModel: CreateReturnOrderDto = this.getEmptyCreateModel();
  newReturnLine: CreateReturnOrderItemDto = this.getEmptyLineModel();
  selectedProductForLine: ProductDto | null = null;

  // Receive Modal State
  isReceiveModalOpen = false;
  selectedReturnForReceive: ReturnOrderDto | null = null;
  receiveModel: ReceiveReturnDto = { items: [] };

  // Inspection & Grading Modal State
  isInspectModalOpen = false;
  selectedReturnForInspect: ReturnOrderDto | null = null;
  inspectModel: InspectReturnOrderDto = { inspectorName: '', items: [] };

  // Transit Dispatch Modal State
  isTransitModalOpen = false;
  selectedReturnForTransit: ReturnOrderDto | null = null;
  transitCarrier = '';
  transitTrackingNumber = '';

  // Details Modal State
  isDetailsModalOpen = false;
  selectedReturnDetails: ReturnOrderDto | null = null;

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadLocations();
    this.loadProducts();
    this.loadCustomers();
    this.loadSuppliers();
    this.loadReturns();
    this.loadMetrics();
  }

  loadWarehouses(): void {
    this.warehouseService.getList({ maxResultCount: 100 }).subscribe({
      next: res => {
        this.warehouses = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  loadLocations(): void {
    this.locationService.getList({ maxResultCount: 500 }).subscribe({
      next: res => {
        this.locations = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  loadProducts(): void {
    this.productService.getList({ maxResultCount: 300 }).subscribe({
      next: res => {
        this.products = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  loadCustomers(): void {
    this.customerService.getList({ maxResultCount: 200 }).subscribe({
      next: res => {
        this.customers = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  loadSuppliers(): void {
    this.supplierService.getList({ maxResultCount: 200 }).subscribe({
      next: res => {
        this.suppliers = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  loadReturns(): void {
    this.isLoading = true;

    let statusFilter: ReturnOrderStatus | undefined = undefined;
    if (this.selectedStatusTab === 'PENDING') {
      statusFilter = ReturnOrderStatus.Draft;
    } else if (this.selectedStatusTab === 'IN_TRANSIT') {
      statusFilter = ReturnOrderStatus.InTransit;
    } else if (this.selectedStatusTab === 'RECEIVED') {
      statusFilter = ReturnOrderStatus.Received;
    } else if (this.selectedStatusTab === 'COMPLETED') {
      statusFilter = ReturnOrderStatus.Completed;
    }

    this.returnService
      .getList({
        filter: this.searchQuery || undefined,
        warehouseId: this.selectedWarehouseFilter || undefined,
        returnType: this.selectedTypeFilter !== '' ? this.selectedTypeFilter : undefined,
        status: statusFilter,
        skipCount: (this.currentPage - 1) * this.pageSize,
        maxResultCount: this.pageSize,
        sorting: 'CreationTime desc',
      })
      .subscribe({
        next: res => {
          this.returns = res.items || [];
          this.totalCount = res.totalCount || 0;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: err => {
          this.isLoading = false;
          this.toasterService.error(this.extractErrorMessage(err, 'Failed to load return orders.'));
          this.cdr.detectChanges();
        },
      });
  }

  loadMetrics(): void {
    this.returnService.getDashboardMetrics(this.selectedWarehouseFilter || undefined).subscribe({
      next: res => {
        this.metrics = res;
        this.cdr.detectChanges();
      },
    });
  }

  // Filter actions
  onSearch(): void {
    this.currentPage = 1;
    this.loadReturns();
    this.loadMetrics();
  }

  onStatusTabChange(tab: 'ALL' | 'PENDING' | 'IN_TRANSIT' | 'RECEIVED' | 'COMPLETED'): void {
    this.selectedStatusTab = tab;
    this.currentPage = 1;
    this.loadReturns();
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.selectedTypeFilter = '';
    this.selectedStatusTab = 'ALL';
    this.selectedWarehouseFilter = '';
    this.currentPage = 1;
    this.loadReturns();
    this.loadMetrics();
  }

  // Pagination
  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize) || 1;
  }

  get paginationStart(): number {
    if (this.totalCount === 0) return 0;
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalCount);
  }

  goToPage(p: number): void {
    if (p >= 1 && p <= this.totalPages) {
      this.currentPage = p;
      this.loadReturns();
    }
  }

  prevPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadReturns();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadReturns();
    }
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize = Number(newSize);
    this.currentPage = 1;
    this.loadReturns();
  }

  // Create Modal Actions
  openCreateModal(): void {
    this.newReturnModel = this.getEmptyCreateModel();
    this.newReturnLine = this.getEmptyLineModel();
    this.selectedProductForLine = null;
    if (this.warehouses.length > 0) {
      this.newReturnModel.warehouseId = this.warehouses[0].id || '';
    }
    this.isCreateModalOpen = true;
  }

  closeCreateModal(): void {
    this.isCreateModalOpen = false;
  }

  onProductSelected(): void {
    const prod = this.products.find(p => p.id === this.newReturnLine.productId);
    this.selectedProductForLine = prod || null;
    if (prod) {
      this.newReturnLine.unitRefundPrice = prod.unitPrice || 0;
    }
  }

  addLineItem(): void {
    if (!this.newReturnLine.productId) {
      this.toasterService.error('Please select a product SKU.');
      return;
    }
    if (!this.newReturnLine.expectedQuantity || this.newReturnLine.expectedQuantity <= 0) {
      this.toasterService.error('Quantity must be greater than zero.');
      return;
    }

    this.newReturnModel.items.push({
      ...this.newReturnLine,
      expectedQuantity: Number(this.newReturnLine.expectedQuantity),
      unitRefundPrice: Number(this.newReturnLine.unitRefundPrice) || 0,
    });

    this.newReturnLine = this.getEmptyLineModel();
    this.selectedProductForLine = null;
  }

  removeLineItem(idx: number): void {
    this.newReturnModel.items.splice(idx, 1);
  }

  saveReturnOrder(): void {
    if (!this.newReturnModel.warehouseId) {
      this.toasterService.error('Please select a receiving warehouse facility.');
      return;
    }
    if (this.newReturnModel.returnType === ReturnType.CustomerReturn && !this.newReturnModel.customerId) {
      this.toasterService.error('Please select a customer for Customer Return.');
      return;
    }
    if (this.newReturnModel.returnType === ReturnType.VendorReturn && !this.newReturnModel.supplierId) {
      this.toasterService.error('Please select a vendor for Supplier Return.');
      return;
    }
    if (this.newReturnModel.items.length === 0) {
      this.toasterService.error('Please add at least one line item to this return order.');
      return;
    }

    // Auto map names
    if (this.newReturnModel.customerId) {
      const c = this.customers.find(x => x.id === this.newReturnModel.customerId);
      this.newReturnModel.customerName = c?.name;
    }
    if (this.newReturnModel.supplierId) {
      const s = this.suppliers.find(x => x.id === this.newReturnModel.supplierId);
      this.newReturnModel.supplierName = s?.name;
    }

    this.isSaving = true;
    this.returnService.create(this.newReturnModel).subscribe({
      next: res => {
        this.isSaving = false;
        this.toasterService.success(`RMA Order ${res.rmaNumber} authorized successfully!`);
        this.closeCreateModal();
        this.loadReturns();
        this.loadMetrics();
      },
      error: err => {
        this.isSaving = false;
        this.toasterService.error(this.extractErrorMessage(err, 'Failed to create RMA order.'));
      },
    });
  }

  // Workflow Actions
  approveRma(order: ReturnOrderDto): void {
    if (!order.id) return;
    if (confirm(`Authorize return order ${order.rmaNumber}?`)) {
      this.returnService.approve(order.id).subscribe({
        next: () => {
          this.toasterService.success(`RMA ${order.rmaNumber} approved for transit.`);
          this.loadReturns();
          this.loadMetrics();
        },
        error: err => {
          this.toasterService.error(this.extractErrorMessage(err, 'Approval failed.'));
        },
      });
    }
  }

  openTransitModal(order: ReturnOrderDto): void {
    this.selectedReturnForTransit = order;
    this.transitCarrier = order.carrierName || 'Delhivery Express';
    this.transitTrackingNumber = order.trackingNumber || `RT-AWB-${Math.floor(100000 + Math.random() * 900000)}`;
    this.isTransitModalOpen = true;
  }

  closeTransitModal(): void {
    this.isTransitModalOpen = false;
    this.selectedReturnForTransit = null;
  }

  submitTransit(): void {
    if (!this.selectedReturnForTransit || !this.selectedReturnForTransit.id) return;
    this.isSaving = true;
    this.returnService
      .markInTransit(this.selectedReturnForTransit.id, {
        carrierName: this.transitCarrier,
        trackingNumber: this.transitTrackingNumber,
      })
      .subscribe({
        next: () => {
          this.isSaving = false;
          this.toasterService.success(`Return marked In-Transit via ${this.transitCarrier}`);
          this.closeTransitModal();
          this.loadReturns();
          this.loadMetrics();
        },
        error: err => {
          this.isSaving = false;
          this.toasterService.error(this.extractErrorMessage(err, 'Failed to update transit status.'));
        },
      });
  }

  openReceiveModal(order: ReturnOrderDto): void {
    this.selectedReturnForReceive = order;
    this.receiveModel = {
      receivedDate: new Date().toISOString().split('T')[0],
      notes: 'Received at receiving dock; packaging inspected.',
      items: (order.items || []).map((item: ReturnOrderItemDto) => ({
        itemId: item.id || '',
        receivedQuantity: item.expectedQuantity,
      })),
    };
    this.isReceiveModalOpen = true;
  }

  closeReceiveModal(): void {
    this.isReceiveModalOpen = false;
    this.selectedReturnForReceive = null;
  }

  submitReceive(): void {
    if (!this.selectedReturnForReceive || !this.selectedReturnForReceive.id) return;
    this.isSaving = true;
    this.returnService.receive(this.selectedReturnForReceive.id, this.receiveModel).subscribe({
      next: () => {
        this.isSaving = false;
        this.toasterService.success(`RMA ${this.selectedReturnForReceive?.rmaNumber} received at warehouse dock!`);
        this.closeReceiveModal();
        this.loadReturns();
        this.loadMetrics();
      },
      error: err => {
        this.isSaving = false;
        this.toasterService.error(this.extractErrorMessage(err, 'Failed to record dock receipt.'));
      },
    });
  }

  openInspectModal(order: ReturnOrderDto): void {
    this.selectedReturnForInspect = order;
    const warehouseBins = this.locations.filter(l => l.warehouseId === order.warehouseId);
    const defaultBin = warehouseBins.length > 0 ? warehouseBins[0].id : undefined;

    this.inspectModel = {
      inspectorName: 'Quality Specialist',
      items: (order.items || []).map((item: ReturnOrderItemDto) => ({
        itemId: item.id || '',
        conditionGrade: ItemConditionGrade.GradeA_BrandNew,
        resolutionAction: ItemResolutionAction.RestockToInventory,
        restockedQuantity: item.receivedQuantity > 0 ? item.receivedQuantity : item.expectedQuantity,
        scrappedQuantity: 0,
        destinationLocationId: defaultBin,
        inspectionNotes: 'Sealed pack, verified good stock.',
      })),
    };
    this.isInspectModalOpen = true;
  }

  closeInspectModal(): void {
    this.isInspectModalOpen = false;
    this.selectedReturnForInspect = null;
  }

  onConditionGradeChange(itemIdx: number): void {
    const item = this.inspectModel.items[itemIdx];
    const totalQty = (this.selectedReturnForInspect?.items[itemIdx]?.receivedQuantity || 
                      this.selectedReturnForInspect?.items[itemIdx]?.expectedQuantity || 1);

    if (item.conditionGrade === ItemConditionGrade.GradeA_BrandNew || item.conditionGrade === ItemConditionGrade.GradeB_OpenBox) {
      item.resolutionAction = ItemResolutionAction.RestockToInventory;
      item.restockedQuantity = totalQty;
      item.scrappedQuantity = 0;
      item.inspectionNotes = 'Grade verified; approved for direct inventory putaway.';
    } else if (item.conditionGrade === ItemConditionGrade.GradeD_Scrap) {
      item.resolutionAction = ItemResolutionAction.ScrapAndWriteOff;
      item.restockedQuantity = 0;
      item.scrappedQuantity = totalQty;
      item.inspectionNotes = 'Unrepairable damage / write-off. Transferred to scrap hold.';
    } else {
      item.resolutionAction = ItemResolutionAction.ReturnToVendor;
      item.restockedQuantity = 0;
      item.scrappedQuantity = totalQty;
      item.inspectionNotes = 'Defective component. Recommended for supplier claim.';
    }
  }

  submitInspection(): void {
    if (!this.selectedReturnForInspect || !this.selectedReturnForInspect.id) return;

    for (const item of this.inspectModel.items) {
      if (item.resolutionAction === ItemResolutionAction.RestockToInventory && item.restockedQuantity > 0) {
        if (!item.destinationLocationId) {
          this.toasterService.error('Please assign a destination bin location for all restocked items.');
          return;
        }
      }
    }

    this.isSaving = true;
    this.returnService.inspectAndResolve(this.selectedReturnForInspect.id, this.inspectModel).subscribe({
      next: () => {
        this.isSaving = false;
        this.toasterService.success(`RMA ${this.selectedReturnForInspect?.rmaNumber} quality grading completed! Physical inventory updated.`);
        this.closeInspectModal();
        this.loadReturns();
        this.loadMetrics();
      },
      error: err => {
        this.isSaving = false;
        this.toasterService.error(this.extractErrorMessage(err, 'Failed to complete quality inspection.'));
      },
    });
  }

  openDetailsModal(order: ReturnOrderDto): void {
    this.selectedReturnDetails = order;
    this.isDetailsModalOpen = true;
  }

  closeDetailsModal(): void {
    this.isDetailsModalOpen = false;
    this.selectedReturnDetails = null;
  }

  printReturnChallan(): void {
    window.print();
  }

  // Helpers
  getAvailableLocationsForWarehouse(whId: string): WarehouseLocationDto[] {
    return this.locations.filter(l => l.warehouseId === whId);
  }

  getProductName(productId: string): string {
    const p = this.products.find(x => x.id === productId);
    return p ? `${p.sku} - ${p.name}` : 'Unknown Product';
  }

  getTypeLabel(type?: ReturnType): string {
    switch (type) {
      case ReturnType.CustomerReturn: return 'Customer Return';
      case ReturnType.VendorReturn: return 'Vendor (RTV)';
      case ReturnType.InterBranchReturn: return 'Branch Return';
      default: return 'Customer Return';
    }
  }

  getReasonLabel(reason: ReturnReason): string {
    switch (reason) {
      case ReturnReason.DefectiveItem: return 'Defective Item';
      case ReturnReason.WrongItemShipped: return 'Wrong SKU Shipped';
      case ReturnReason.DamagedInTransit: return 'Transit Damage';
      case ReturnReason.CustomerCancellation: return 'Customer Cancelled';
      case ReturnReason.QualityDiscrepancy: return 'Quality Mismatch';
      case ReturnReason.ExpiredProduct: return 'Expired Product';
      case ReturnReason.ExcessStock: return 'Excess Inventory';
      default: return 'Other Reason';
    }
  }

  getStatusBadgeClass(status: ReturnOrderStatus): string {
    switch (status) {
      case ReturnOrderStatus.Draft: return 'badge bg-secondary';
      case ReturnOrderStatus.Requested: return 'badge bg-warning text-dark';
      case ReturnOrderStatus.Approved: return 'badge bg-primary';
      case ReturnOrderStatus.InTransit: return 'badge bg-info text-dark';
      case ReturnOrderStatus.Received: return 'badge bg-indigo text-white';
      case ReturnOrderStatus.UnderInspection: return 'badge bg-warning text-dark';
      case ReturnOrderStatus.Completed: return 'badge bg-success';
      case ReturnOrderStatus.Rejected: return 'badge bg-danger';
      case ReturnOrderStatus.Cancelled: return 'badge bg-dark';
      default: return 'badge bg-light text-dark';
    }
  }

  getStatusLabel(status: ReturnOrderStatus): string {
    switch (status) {
      case ReturnOrderStatus.Draft: return 'Draft';
      case ReturnOrderStatus.Requested: return 'Requested';
      case ReturnOrderStatus.Approved: return 'RMA Authorized';
      case ReturnOrderStatus.InTransit: return 'In-Transit (RTO)';
      case ReturnOrderStatus.Received: return 'Dock Received';
      case ReturnOrderStatus.UnderInspection: return 'In QC Inspection';
      case ReturnOrderStatus.Completed: return 'Restocked & Closed';
      case ReturnOrderStatus.Rejected: return 'Rejected';
      case ReturnOrderStatus.Cancelled: return 'Cancelled';
      default: return 'Unknown';
    }
  }

  getConditionBadge(grade?: ItemConditionGrade): string {
    switch (grade) {
      case ItemConditionGrade.GradeA_BrandNew: return 'badge bg-success text-white';
      case ItemConditionGrade.GradeB_OpenBox: return 'badge bg-info text-white';
      case ItemConditionGrade.GradeC_Defective: return 'badge bg-warning text-dark';
      case ItemConditionGrade.GradeD_Scrap: return 'badge bg-danger text-white';
      default: return 'badge bg-secondary';
    }
  }

  getResolutionBadge(action?: ItemResolutionAction): string {
    switch (action) {
      case ItemResolutionAction.RestockToInventory: return 'badge bg-success-subtle text-success border border-success';
      case ItemResolutionAction.ScrapAndWriteOff: return 'badge bg-danger-subtle text-danger border border-danger';
      case ItemResolutionAction.ReturnToVendor: return 'badge bg-warning-subtle text-warning-emphasis border border-warning';
      case ItemResolutionAction.QuarantineHold: return 'badge bg-info-subtle text-info-emphasis border border-info';
      default: return 'badge bg-light text-muted border';
    }
  }

  private getEmptyCreateModel(): CreateReturnOrderDto {
    return {
      returnType: ReturnType.CustomerReturn,
      warehouseId: '',
      customerId: undefined,
      customerName: '',
      supplierId: undefined,
      supplierName: '',
      outboundOrderId: undefined,
      outboundOrderNumber: '',
      inboundOrderId: undefined,
      inboundOrderNumber: '',
      carrierName: '',
      trackingNumber: '',
      returnDate: new Date().toISOString().split('T')[0],
      notes: '',
      items: [],
    };
  }

  private getEmptyLineModel(): CreateReturnOrderItemDto {
    return {
      productId: '',
      expectedQuantity: 1,
      unitRefundPrice: 0,
      returnReason: ReturnReason.DefectiveItem,
      lotBatchNumber: '',
      serialNumber: '',
    };
  }

  private extractErrorMessage(err: any, fallback: string = 'Operation failed'): string {
    if (!err) return fallback;
    const errObj = err.error?.error || err.error;
    if (errObj?.validationErrors && Array.isArray(errObj.validationErrors) && errObj.validationErrors.length > 0) {
      return errObj.validationErrors.map((v: any) => v.message).filter(Boolean).join(' | ');
    }
    if (errObj?.details) return errObj.details;
    if (errObj?.message) return errObj.message;
    if (err.message) return err.message;
    return fallback;
  }
}
