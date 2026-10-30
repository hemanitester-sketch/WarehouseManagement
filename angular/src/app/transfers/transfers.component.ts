import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { StockTransferService } from '../proxy/transfers/stock-transfer.service';
import {
  StockTransferDto,
  StockTransferItemDto,
  CreateUpdateStockTransferDto,
  CreateStockTransferItemDto,
  DispatchTransferDto,
  TransferType,
  TransferStatus,
  transferTypeOptions,
  transferStatusOptions,
} from '../proxy/transfers/models';
import { WarehouseService, WarehouseLocationService } from '../proxy/warehouses';
import { ProductService } from '../proxy/products';
import { WarehouseDto, WarehouseLocationDto } from '../proxy/warehouses/models';
import { ProductDto } from '../proxy/products/models';

@Component({
  selector: 'app-transfers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transfers.component.html',
  styleUrls: ['./transfers.component.scss'],
})
export class TransfersComponent implements OnInit {
  private transferService = inject(StockTransferService);
  private warehouseService = inject(WarehouseService);
  private locationService = inject(WarehouseLocationService);
  private productService = inject(ProductService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  transfers: StockTransferDto[] = [];
  warehouses: WarehouseDto[] = [];
  products: ProductDto[] = [];
  allLocations: WarehouseLocationDto[] = [];

  isLoading = false;
  isSaving = false;

  // Search & Filters
  searchQuery = '';
  typeFilter: string = '';
  statusFilter: string = '';
  sourceWarehouseFilter: string = '';
  destinationWarehouseFilter: string = '';

  typeOptions = transferTypeOptions;
  statusOptions = transferStatusOptions;

  // Pagination
  currentPage = 1;
  pageSize = 50;

  // Create / Edit Modal State
  isCreateModalOpen = false;
  transferModel: CreateUpdateStockTransferDto = {
    transferNumber: '',
    type: TransferType.InterWarehouse,
    sourceWarehouseId: '',
    destinationWarehouseId: '',
    carrierName: '',
    trackingNumber: '',
    vehicleNumber: '',
    driverName: '',
    driverPhone: '',
    notes: '',
    transferDate: new Date().toISOString().split('T')[0],
    deliveryChallanNumber: '',
    items: [],
  };

  // Dispatch Modal State
  isDispatchModalOpen = false;
  selectedTransferForDispatch: StockTransferDto | null = null;
  dispatchModel: DispatchTransferDto = {
    carrierName: '',
    trackingNumber: '',
    vehicleNumber: '',
    driverName: '',
    driverPhone: '',
    deliveryChallanNumber: '',
  };

  // Cancel Modal State
  isCancelModalOpen = false;
  selectedTransferForCancel: StockTransferDto | null = null;
  cancelReason = '';

  // Challan / Gate Pass Modal State
  isChallanModalOpen = false;
  selectedTransferForChallan: StockTransferDto | null = null;

  // Details Modal State
  isDetailsModalOpen = false;
  selectedTransferDetails: StockTransferDto | null = null;

  // Filtered Locations for Source and Destination
  sourceLocations: WarehouseLocationDto[] = [];
  destinationLocations: WarehouseLocationDto[] = [];

  // KPIs
  get totalTransfersCount(): number {
    return this.transfers.length;
  }

  get inTransitCount(): number {
    return this.transfers.filter(t => t.status === TransferStatus.InTransit).length;
  }

  get pendingCount(): number {
    return this.transfers.filter(
      t => t.status === TransferStatus.Draft || t.status === TransferStatus.Approved
    ).length;
  }

  get completedCount(): number {
    return this.transfers.filter(t => t.status === TransferStatus.Completed).length;
  }

  // Filtered transfers for table
  get filteredTransfers(): StockTransferDto[] {
    return this.transfers.filter(t => {
      const q = this.searchQuery.trim().toLowerCase();
      const matchesSearch =
        !q ||
        t.transferNumber?.toLowerCase().includes(q) ||
        t.carrierName?.toLowerCase().includes(q) ||
        t.trackingNumber?.toLowerCase().includes(q) ||
        t.vehicleNumber?.toLowerCase().includes(q) ||
        t.driverName?.toLowerCase().includes(q) ||
        t.deliveryChallanNumber?.toLowerCase().includes(q) ||
        t.notes?.toLowerCase().includes(q);

      const matchesType =
        this.typeFilter === '' || t.type.toString() === this.typeFilter;

      const matchesStatus =
        this.statusFilter === '' || t.status.toString() === this.statusFilter;

      const matchesSource =
        !this.sourceWarehouseFilter || t.sourceWarehouseId === this.sourceWarehouseFilter;

      const matchesDest =
        !this.destinationWarehouseFilter || t.destinationWarehouseId === this.destinationWarehouseFilter;

      return matchesSearch && matchesType && matchesStatus && matchesSource && matchesDest;
    });
  }

  get paginatedTransfers(): StockTransferDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredTransfers.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredTransfers.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    return this.filteredTransfers.length === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredTransfers.length);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  prevPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize = Number(newSize);
    this.currentPage = 1;
  }

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadProducts();
    this.loadAllLocations();
    this.loadTransfers();
  }

  loadWarehouses(): void {
    this.warehouseService.getList({ maxResultCount: 100 }).subscribe({
      next: res => {
        this.warehouses = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  loadProducts(): void {
    this.productService.getList({ maxResultCount: 200 }).subscribe({
      next: res => {
        this.products = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  loadAllLocations(): void {
    this.locationService.getList({ maxResultCount: 500 }).subscribe({
      next: res => {
        this.allLocations = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  loadTransfers(): void {
    this.isLoading = true;
    this.transferService.getList({ maxResultCount: 200 }).subscribe({
      next: res => {
        this.transfers = res.items || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  onSearch(): void {
    this.currentPage = 1;
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.typeFilter = '';
    this.statusFilter = '';
    this.sourceWarehouseFilter = '';
    this.destinationWarehouseFilter = '';
    this.currentPage = 1;
  }

  // Create Modal Actions
  openCreateModal(): void {
    const today = new Date().toISOString().split('T')[0];
    const defaultSrc = this.warehouses.length > 0 ? (this.warehouses[0].id || '') : '';
    const defaultDst = this.warehouses.length > 1 ? (this.warehouses[1].id || '') : defaultSrc;

    this.transferModel = {
      transferNumber: `TRF-${Math.floor(100000 + Math.random() * 900000)}`,
      type: TransferType.InterWarehouse,
      sourceWarehouseId: defaultSrc,
      destinationWarehouseId: defaultDst,
      carrierName: '',
      trackingNumber: '',
      vehicleNumber: '',
      driverName: '',
      driverPhone: '',
      notes: '',
      transferDate: today,
      deliveryChallanNumber: `DC-${Math.floor(10000 + Math.random() * 90000)}`,
      items: [],
    };

    this.updateSourceLocations();
    this.updateDestinationLocations();
    this.addItemRow();
    this.isCreateModalOpen = true;
  }

  closeCreateModal(): void {
    this.isCreateModalOpen = false;
  }

  onSourceWarehouseChange(): void {
    this.updateSourceLocations();
    if (this.transferModel.type === TransferType.BinToBin || this.transferModel.type === TransferType.Replenishment) {
      this.transferModel.destinationWarehouseId = this.transferModel.sourceWarehouseId;
      this.updateDestinationLocations();
    }
  }

  onDestinationWarehouseChange(): void {
    this.updateDestinationLocations();
  }

  onTypeChange(): void {
    if (this.transferModel.type === TransferType.BinToBin || this.transferModel.type === TransferType.Replenishment) {
      this.transferModel.destinationWarehouseId = this.transferModel.sourceWarehouseId;
      this.updateDestinationLocations();
    }
  }

  updateSourceLocations(): void {
    if (this.transferModel.sourceWarehouseId) {
      this.sourceLocations = this.allLocations.filter(
        l => l.warehouseId === this.transferModel.sourceWarehouseId
      );
    } else {
      this.sourceLocations = [];
    }
  }

  updateDestinationLocations(): void {
    if (this.transferModel.destinationWarehouseId) {
      this.destinationLocations = this.allLocations.filter(
        l => l.warehouseId === this.transferModel.destinationWarehouseId
      );
    } else {
      this.destinationLocations = [];
    }
  }

  addItemRow(): void {
    const defaultProduct = this.products.length > 0 ? this.products[0] : null;
    this.transferModel.items.push({
      productId: defaultProduct ? (defaultProduct.id || '') : '',
      sourceLocationId: this.sourceLocations.length > 0 ? (this.sourceLocations[0].id || '') : '',
      destinationLocationId: this.destinationLocations.length > 0 ? (this.destinationLocations[0].id || '') : '',
      requestedQuantity: 10,
      unitCost: defaultProduct ? (defaultProduct.unitPrice || 0) : 0,
      notes: '',
    });
  }

  removeItemRow(index: number): void {
    if (this.transferModel.items.length > 1) {
      this.transferModel.items.splice(index, 1);
    } else {
      this.toasterService.warn('A transfer order must contain at least one item line.');
    }
  }

  onProductChange(item: CreateStockTransferItemDto): void {
    const prod = this.products.find(p => p.id === item.productId);
    if (prod) {
      item.unitCost = prod.unitPrice || 0;
    }
  }

  get totalUnitsCalculated(): number {
    return this.transferModel.items.reduce((acc, cur) => acc + (Number(cur.requestedQuantity) || 0), 0);
  }

  saveTransfer(): void {
    if (!this.transferModel.sourceWarehouseId) {
      this.toasterService.error('Please select a source warehouse.');
      return;
    }
    if (!this.transferModel.destinationWarehouseId) {
      this.toasterService.error('Please select a destination warehouse.');
      return;
    }
    if (this.transferModel.items.length === 0) {
      this.toasterService.error('Please add at least one line item.');
      return;
    }
    for (const item of this.transferModel.items) {
      if (!item.productId) {
        this.toasterService.error('Each line item must have a valid product.');
        return;
      }
      if (!item.requestedQuantity || item.requestedQuantity <= 0) {
        this.toasterService.error('Requested quantity must be greater than zero.');
        return;
      }
    }

    this.isSaving = true;
    this.transferService.create(this.transferModel).subscribe({
      next: res => {
        this.isSaving = false;
        this.toasterService.success(`Transfer ${res.transferNumber} created successfully!`);
        this.closeCreateModal();
        this.loadTransfers();
      },
      error: err => {
        this.isSaving = false;
        this.toasterService.error(err.message || 'Failed to create transfer order.');
      },
    });
  }

  // Workflow Actions
  approveTransfer(t: StockTransferDto): void {
    if (confirm(`Approve transfer order ${t.transferNumber}?`)) {
      this.transferService.approve(t.id).subscribe({
        next: () => {
          this.toasterService.success(`Transfer ${t.transferNumber} approved.`);
          this.loadTransfers();
        },
        error: err => {
          this.toasterService.error(err.message || 'Failed to approve transfer.');
        },
      });
    }
  }

  openDispatchModal(t: StockTransferDto): void {
    this.selectedTransferForDispatch = t;
    this.dispatchModel = {
      carrierName: t.carrierName || 'BlueDart Express Logistics',
      trackingNumber: t.trackingNumber || `TRK-BD-${Math.floor(100000 + Math.random() * 900000)}`,
      vehicleNumber: t.vehicleNumber || 'MH-04-AZ-8899',
      driverName: t.driverName || 'Ramesh Kumar',
      driverPhone: t.driverPhone || '+91 98200 44551',
      deliveryChallanNumber: t.deliveryChallanNumber || `DC-${Math.floor(10000 + Math.random() * 90000)}`,
    };
    this.isDispatchModalOpen = true;
  }

  closeDispatchModal(): void {
    this.isDispatchModalOpen = false;
    this.selectedTransferForDispatch = null;
  }

  confirmDispatch(): void {
    if (!this.selectedTransferForDispatch) return;
    this.isSaving = true;
    this.transferService.dispatch(this.selectedTransferForDispatch.id, this.dispatchModel).subscribe({
      next: () => {
        this.isSaving = false;
        this.toasterService.success(
          `Transfer ${this.selectedTransferForDispatch?.transferNumber} dispatched in transit!`
        );
        this.closeDispatchModal();
        this.loadTransfers();
      },
      error: err => {
        this.isSaving = false;
        this.toasterService.error(err.message || 'Failed to dispatch transfer.');
      },
    });
  }

  completeTransfer(t: StockTransferDto): void {
    if (
      confirm(
        `Confirm receipt and putaway for ${t.transferNumber} at destination warehouse? This will automatically update physical inventory balances.`
      )
    ) {
      this.transferService.complete(t.id).subscribe({
        next: () => {
          this.toasterService.success(
            `Transfer ${t.transferNumber} completed! Inventory relocated and stock balances updated.`
          );
          this.loadTransfers();
        },
        error: err => {
          this.toasterService.error(err.message || 'Failed to complete transfer.');
        },
      });
    }
  }

  openCancelModal(t: StockTransferDto): void {
    this.selectedTransferForCancel = t;
    this.cancelReason = 'Order cancelled by logistics controller';
    this.isCancelModalOpen = true;
  }

  closeCancelModal(): void {
    this.isCancelModalOpen = false;
    this.selectedTransferForCancel = null;
  }

  confirmCancel(): void {
    if (!this.selectedTransferForCancel) return;
    this.transferService.cancel(this.selectedTransferForCancel.id, this.cancelReason).subscribe({
      next: () => {
        this.toasterService.info(`Transfer ${this.selectedTransferForCancel?.transferNumber} cancelled.`);
        this.closeCancelModal();
        this.loadTransfers();
      },
      error: err => {
        this.toasterService.error(err.message || 'Failed to cancel transfer.');
      },
    });
  }

  // Challan / Gate Pass
  openChallanModal(t: StockTransferDto): void {
    this.selectedTransferForChallan = t;
    this.isChallanModalOpen = true;
  }

  closeChallanModal(): void {
    this.isChallanModalOpen = false;
    this.selectedTransferForChallan = null;
  }

  printChallan(): void {
    window.print();
  }

  // Details Modal
  openDetailsModal(t: StockTransferDto): void {
    this.selectedTransferDetails = t;
    this.isDetailsModalOpen = true;
  }

  closeDetailsModal(): void {
    this.isDetailsModalOpen = false;
    this.selectedTransferDetails = null;
  }

  // UI Helpers
  getTypeBadgeClass(type: TransferType): string {
    switch (type) {
      case TransferType.InterWarehouse:
        return 'type-interwh';
      case TransferType.BinToBin:
        return 'type-bintobin';
      case TransferType.Replenishment:
        return 'type-replenish';
      case TransferType.Quarantine:
        return 'type-quarantine';
      default:
        return 'type-default';
    }
  }

  getTypeLabel(type: TransferType): string {
    switch (type) {
      case TransferType.InterWarehouse:
        return 'Inter-Warehouse';
      case TransferType.BinToBin:
        return 'Bin-to-Bin Move';
      case TransferType.Replenishment:
        return 'Replenishment';
      case TransferType.Quarantine:
        return 'Quarantine / Hold';
      default:
        return 'Transfer';
    }
  }

  getTypeIcon(type: TransferType): string {
    switch (type) {
      case TransferType.InterWarehouse:
        return 'bi bi-truck';
      case TransferType.BinToBin:
        return 'bi bi-arrow-left-right';
      case TransferType.Replenishment:
        return 'bi bi-arrow-up-circle';
      case TransferType.Quarantine:
        return 'bi bi-shield-exclamation';
      default:
        return 'bi bi-arrow-repeat';
    }
  }

  getStatusBadgeClass(status: TransferStatus): string {
    switch (status) {
      case TransferStatus.Draft:
        return 'status-draft';
      case TransferStatus.Approved:
        return 'status-approved';
      case TransferStatus.InTransit:
        return 'status-intransit';
      case TransferStatus.Completed:
        return 'status-completed';
      case TransferStatus.Cancelled:
        return 'status-cancelled';
      default:
        return 'status-default';
    }
  }

  getStatusLabel(status: TransferStatus): string {
    switch (status) {
      case TransferStatus.Draft:
        return 'Draft';
      case TransferStatus.Approved:
        return 'Approved';
      case TransferStatus.InTransit:
        return 'In Transit';
      case TransferStatus.Completed:
        return 'Completed';
      case TransferStatus.Cancelled:
        return 'Cancelled';
      default:
        return 'Unknown';
    }
  }

  getStatusIcon(status: TransferStatus): string {
    switch (status) {
      case TransferStatus.Draft:
        return 'bi bi-pencil';
      case TransferStatus.Approved:
        return 'bi bi-check-circle';
      case TransferStatus.InTransit:
        return 'bi bi-truck';
      case TransferStatus.Completed:
        return 'bi bi-check2-all';
      case TransferStatus.Cancelled:
        return 'bi bi-x-circle';
      default:
        return 'bi bi-circle';
    }
  }
}
