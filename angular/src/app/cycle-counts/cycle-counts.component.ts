import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService, ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import {
  CycleCountAuditService,
  CycleCountAuditDto,
  CycleCountAuditItemDto,
  CycleCountType,
  CycleCountStatus,
  DiscrepancyReason,
  CreateCycleCountAuditDto,
  CreateCycleCountAuditItemDto,
  RecordCountItemDto,
  CycleCountDashboardMetricsDto,
} from '../proxy/audits';
import { WarehouseService, WarehouseZoneService, WarehouseLocationService } from '../proxy/warehouses';
import { ProductService } from '../proxy/products';
import { FacultyMemberService } from '../proxy/faculty';
import { WarehouseDto, WarehouseZoneDto, WarehouseLocationDto } from '../proxy/warehouses/models';
import { ProductDto } from '../proxy/products/models';
import { FacultyMemberDto } from '../proxy/faculty/models';

@Component({
  selector: 'app-cycle-counts',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cycle-counts.component.html',
  styleUrls: ['./cycle-counts.component.scss'],
})
export class CycleCountsComponent implements OnInit {
  private auditService = inject(CycleCountAuditService);
  private warehouseService = inject(WarehouseService);
  private zoneService = inject(WarehouseZoneService);
  private locationService = inject(WarehouseLocationService);
  private productService = inject(ProductService);
  private facultyService = inject(FacultyMemberService);
  private toasterService = inject(ToasterService);
  private confirmationService = inject(ConfirmationService);
  private cdr = inject(ChangeDetectorRef);

  // Enums for template
  CycleCountType = CycleCountType;
  CycleCountStatus = CycleCountStatus;
  DiscrepancyReason = DiscrepancyReason;

  // Master Data
  audits: CycleCountAuditDto[] = [];
  warehouses: WarehouseDto[] = [];
  zones: WarehouseZoneDto[] = [];
  locations: WarehouseLocationDto[] = [];
  products: ProductDto[] = [];
  facultyMembers: FacultyMemberDto[] = [];

  // Metrics KPI
  metrics: CycleCountDashboardMetricsDto = {
    totalAudits: 0,
    inProgressCounts: 0,
    auditsUnderReview: 0,
    reconciledAudits: 0,
    overallAccuracyRate: 100,
    totalNetVarianceUnits: 0,
    totalNetVarianceValue: 0,
  };

  // Filters & State
  isLoading = false;
  filterText = '';
  selectedWarehouseId = '';
  selectedCountType: CycleCountType | '' = '';
  selectedStatusTab: 'ALL' | 'IN_PROGRESS' | 'UNDER_REVIEW' | 'RECONCILED' | 'DRAFT' = 'ALL';
  totalCount = 0;
  page = 1;
  pageSize = 15;

  // Create Modal
  isCreateModalOpen = false;
  isSavingAudit = false;
  isLoadingSnapshot = false;
  newAuditModel: CreateCycleCountAuditDto = {
    warehouseId: '',
    zoneId: '',
    specificProductId: '',
    countType: CycleCountType.ZoneLocationSweep,
    isBlindCount: true,
    scheduledDate: new Date().toISOString().split('T')[0],
    assignedToFacultyId: '',
    assignedAuditorName: '',
    notes: '',
    items: [],
  };
  previewSnapshotItems: CreateCycleCountAuditItemDto[] = [];

  // Active Floor Count Modal
  isCountModalOpen = false;
  activeAuditForCount: CycleCountAuditDto | null = null;
  countItemsDraft: { [itemId: string]: { countedQuantity?: number; reason: DiscrepancyReason; notes?: string } } = {};
  itemFilterSearch = '';
  isSavingCounts = false;

  // Variance Review & Reconcile Modal
  isReconcileModalOpen = false;
  activeAuditForReconcile: CycleCountAuditDto | null = null;
  approverName = '';
  approvalNotes = '';
  isReconciling = false;

  // Printable Count Sheet
  isPrintModalOpen = false;
  activeAuditForPrint: CycleCountAuditDto | null = null;

  // Dropdown Options
  typeOptions = [
    { value: CycleCountType.ZoneLocationSweep, label: 'Zone & Location Sweep (Aisle by Aisle)' },
    { value: CycleCountType.ABCCategory, label: 'ABC Velocity Class Audit (High-Value SKUs)' },
    { value: CycleCountType.ProductSpecific, label: 'Specific SKU Spot Check (Multi-Bin Audit)' },
    { value: CycleCountType.RandomSampling, label: 'Random Sampling (Statistical Verification)' },
    { value: CycleCountType.FullWallToWall, label: 'Full Wall-to-Wall Physical Inventory Count' },
  ];

  discrepancyReasons = [
    { value: DiscrepancyReason.None, label: 'None (Matched / Accurate)' },
    { value: DiscrepancyReason.ShrinkageOrTheft, label: 'Shrinkage / Unaccounted Shortage' },
    { value: DiscrepancyReason.MisplacedInWrongBin, label: 'Misplaced in Adjacent / Wrong Bin' },
    { value: DiscrepancyReason.DamagedOrExpired, label: 'Damaged / Expired Goods Write-off' },
    { value: DiscrepancyReason.DataEntryMismatch, label: 'Data Entry / UOM Conversion Mismatch' },
    { value: DiscrepancyReason.InboundShortage, label: 'Inbound Dock Receiving Shortage' },
    { value: DiscrepancyReason.PickingError, label: 'Outbound Picking / Staging Overage/Shortage' },
    { value: DiscrepancyReason.Other, label: 'Other Operational Discrepancy' },
  ];

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadProducts();
    this.loadLocations();
    this.loadFaculty();
    this.fetchAudits();
    this.fetchMetrics();
  }

  loadLocations(): void {
    this.locationService.getList({ maxResultCount: 500 }).subscribe({
      next: res => {
        this.locations = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  getProductName(productId: string): string {
    const p = this.products.find(x => x.id === productId);
    return p ? `${p.name} (${p.sku})` : (productId ? productId.substring(0, 8) + '...' : '-');
  }

  getLocationName(locationId: string): string {
    const l = this.locations.find(x => x.id === locationId);
    return l ? (l.barcode || `${l.aisle}-${l.rack}-${l.shelf}-${l.bin}`) : (locationId ? locationId.substring(0, 8) + '...' : '-');
  }

  private extractErrorMessage(err: any, fallback: string = 'Operation failed'): string {
    if (!err) return fallback;
    const errObj = err.error?.error || err.error;
    if (errObj?.validationErrors && Array.isArray(errObj.validationErrors) && errObj.validationErrors.length > 0) {
      return errObj.validationErrors.map((v: any) => v.message).filter(Boolean).join(' | ');
    }
    if (errObj?.details) {
      return errObj.details;
    }
    if (errObj?.message) {
      return errObj.message;
    }
    if (err.message) {
      return err.message;
    }
    return fallback;
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

  loadFaculty(): void {
    this.facultyService.getList({ maxResultCount: 100 }).subscribe({
      next: res => {
        this.facultyMembers = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  onWarehouseChangeForCreate(): void {
    this.newAuditModel.zoneId = '';
    this.previewSnapshotItems = [];
    if (!this.newAuditModel.warehouseId) {
      this.zones = [];
      return;
    }

    this.zoneService.getList({ warehouseId: this.newAuditModel.warehouseId, maxResultCount: 100 } as any).subscribe({
      next: res => {
        this.zones = res.items || [];
        this.cdr.detectChanges();
      },
    });
  }

  onAuditorChange(): void {
    const selected = this.facultyMembers.find(f => f.id === this.newAuditModel.assignedToFacultyId);
    this.newAuditModel.assignedAuditorName = selected ? `${selected.fullName} (${selected.designation || 'Auditor'})` : '';
  }

  fetchMetrics(): void {
    this.auditService.getDashboardMetrics(this.selectedWarehouseId || undefined).subscribe({
      next: metrics => {
        this.metrics = metrics;
        this.cdr.detectChanges();
      },
    });
  }

  fetchAudits(): void {
    this.isLoading = true;
    let statusFilter: CycleCountStatus | undefined;
    if (this.selectedStatusTab === 'IN_PROGRESS') {
      statusFilter = CycleCountStatus.InProgress;
    } else if (this.selectedStatusTab === 'UNDER_REVIEW') {
      statusFilter = CycleCountStatus.UnderReview;
    } else if (this.selectedStatusTab === 'RECONCILED') {
      statusFilter = CycleCountStatus.Reconciled;
    } else if (this.selectedStatusTab === 'DRAFT') {
      statusFilter = CycleCountStatus.Draft;
    }

    this.auditService
      .getList({
        filter: this.filterText.trim() || undefined,
        warehouseId: this.selectedWarehouseId || undefined,
        countType: this.selectedCountType !== '' ? this.selectedCountType : undefined,
        status: statusFilter,
        skipCount: (this.page - 1) * this.pageSize,
        maxResultCount: this.pageSize,
        sorting: 'CreationTime desc',
      })
      .subscribe({
        next: res => {
          this.audits = res.items || [];
          this.totalCount = res.totalCount ?? 0;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: err => {
          this.isLoading = false;
          this.toasterService.error(err.error?.message || 'Failed to load stock audits');
          this.cdr.detectChanges();
        },
      });
  }

  onFilterChange(): void {
    this.page = 1;
    this.fetchAudits();
    this.fetchMetrics();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize) || 1;
  }

  get paginationStart(): number {
    return this.totalCount === 0 ? 0 : (this.page - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.page * this.pageSize, this.totalCount);
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page--;
      this.fetchAudits();
    }
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.fetchAudits();
    }
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize = Number(newSize);
    this.page = 1;
    this.fetchAudits();
  }

  setTab(tab: 'ALL' | 'IN_PROGRESS' | 'UNDER_REVIEW' | 'RECONCILED' | 'DRAFT'): void {
    this.selectedStatusTab = tab;
    this.page = 1;
    this.fetchAudits();
  }

  // --- PLAN NEW AUDIT MODAL ---
  openCreateModal(): void {
    this.newAuditModel = {
      warehouseId: this.warehouses.length ? (this.warehouses[0].id || '') : '',
      zoneId: '',
      specificProductId: '',
      countType: CycleCountType.ZoneLocationSweep,
      isBlindCount: true,
      scheduledDate: new Date().toISOString().split('T')[0],
      assignedToFacultyId: '',
      assignedAuditorName: '',
      notes: '',
      items: [],
    };
    this.previewSnapshotItems = [];
    this.onWarehouseChangeForCreate();
    this.isCreateModalOpen = true;
  }

  closeCreateModal(): void {
    this.isCreateModalOpen = false;
  }

  loadLiveSnapshotPreview(): void {
    if (!this.newAuditModel.warehouseId) {
      this.toasterService.warn('Please select a warehouse first.');
      return;
    }

    this.isLoadingSnapshot = true;
    this.auditService
      .generateSnapshotItems(
        this.newAuditModel.warehouseId,
        this.newAuditModel.countType,
        this.newAuditModel.zoneId || undefined,
        this.newAuditModel.specificProductId || undefined
      )
      .subscribe({
        next: items => {
          this.previewSnapshotItems = items || [];
          this.isLoadingSnapshot = false;
          if (this.previewSnapshotItems.length === 0) {
            this.toasterService.info('No on-hand stock found matching the selected criteria in this facility.');
          } else {
            this.toasterService.success(`Loaded ${this.previewSnapshotItems.length} snapshot inventory lines for counting.`);
          }
          this.cdr.detectChanges();
        },
        error: err => {
          this.isLoadingSnapshot = false;
          const msg = this.extractErrorMessage(err, 'Failed to generate inventory snapshot.');
          this.toasterService.error(msg);
          this.cdr.detectChanges();
        },
      });
  }

  saveCycleCount(): void {
    if (!this.newAuditModel.warehouseId) {
      this.toasterService.warn('Please select a warehouse facility.');
      return;
    }

    this.isSavingAudit = true;
    this.newAuditModel.items = this.previewSnapshotItems;

    this.auditService.create(this.newAuditModel).subscribe({
      next: res => {
        this.isSavingAudit = false;
        this.closeCreateModal();
        this.toasterService.success(`Stock Audit Order ${res.auditNumber} successfully created!`);
        this.fetchAudits();
        this.fetchMetrics();
      },
      error: err => {
        this.isSavingAudit = false;
        const msg = this.extractErrorMessage(err, 'Failed to create stock audit.');
        this.toasterService.error(msg);
        this.cdr.detectChanges();
      },
    });
  }

  // --- FLOOR COUNTING MODAL ---
  openCountModal(audit: CycleCountAuditDto): void {
    this.activeAuditForCount = audit;
    this.countItemsDraft = {};
    this.itemFilterSearch = '';

    // If audit was Draft, automatically start it
    if (audit.status === CycleCountStatus.Draft) {
      this.auditService.startCounting(audit.id!).subscribe({
        next: updated => {
          this.activeAuditForCount = updated;
          this.initializeCountDraft(updated);
          this.isCountModalOpen = true;
          this.fetchAudits();
        },
        error: err => {
          this.toasterService.error(err.error?.message || 'Failed to start counting session.');
        },
      });
    } else {
      this.initializeCountDraft(audit);
      this.isCountModalOpen = true;
    }
  }

  private initializeCountDraft(audit: CycleCountAuditDto): void {
    audit.items.forEach(item => {
      this.countItemsDraft[item.id] = {
        countedQuantity: item.countedQuantity,
        reason: item.discrepancyReason || DiscrepancyReason.None,
        notes: item.reasonNotes || '',
      };
    });
  }

  closeCountModal(): void {
    this.isCountModalOpen = false;
    this.activeAuditForCount = null;
  }

  get filteredCountItems(): CycleCountAuditItemDto[] {
    if (!this.activeAuditForCount) return [];
    if (!this.itemFilterSearch.trim()) return this.activeAuditForCount.items;

    const q = this.itemFilterSearch.toLowerCase();
    return this.activeAuditForCount.items.filter(
      itm =>
        itm.productName.toLowerCase().includes(q) ||
        itm.productSku.toLowerCase().includes(q) ||
        itm.locationCode.toLowerCase().includes(q) ||
        (itm.productBarcode && itm.productBarcode.toLowerCase().includes(q))
    );
  }

  saveDraftCounts(): void {
    if (!this.activeAuditForCount) return;

    const payload: RecordCountItemDto[] = [];
    for (const itemId of Object.keys(this.countItemsDraft)) {
      const draft = this.countItemsDraft[itemId];
      if (draft.countedQuantity !== undefined && draft.countedQuantity !== null) {
        payload.push({
          itemId,
          countedQuantity: Number(draft.countedQuantity),
          discrepancyReason: draft.reason,
          reasonNotes: draft.notes,
        });
      }
    }

    if (!payload.length) {
      this.toasterService.warn('No count quantities entered yet.');
      return;
    }

    this.isSavingCounts = true;
    this.auditService.recordCounts(this.activeAuditForCount.id!, payload).subscribe({
      next: res => {
        this.isSavingCounts = false;
        this.activeAuditForCount = res;
        this.toasterService.success('Floor count records saved successfully.');
        this.fetchAudits();
        this.fetchMetrics();
      },
      error: err => {
        this.isSavingCounts = false;
        this.toasterService.error(err.error?.message || 'Failed to save counts.');
        this.cdr.detectChanges();
      },
    });
  }

  submitCountForReview(): void {
    if (!this.activeAuditForCount) return;

    // Check if any items are uncounted
    const uncounted = this.activeAuditForCount.items.filter(i => {
      const d = this.countItemsDraft[i.id];
      return d === undefined || d.countedQuantity === undefined || d.countedQuantity === null;
    });

    if (uncounted.length > 0) {
      this.toasterService.warn(`Cannot submit: ${uncounted.length} item(s) are still uncounted. Please enter physical quantities for all items.`);
      return;
    }

    // First save the current draft records
    const payload: RecordCountItemDto[] = Object.keys(this.countItemsDraft).map(itemId => ({
      itemId,
      countedQuantity: Number(this.countItemsDraft[itemId].countedQuantity),
      discrepancyReason: this.countItemsDraft[itemId].reason,
      reasonNotes: this.countItemsDraft[itemId].notes,
    }));

    this.isSavingCounts = true;
    this.auditService.recordCounts(this.activeAuditForCount.id!, payload).subscribe({
      next: () => {
        this.auditService.submitForReview(this.activeAuditForCount!.id!).subscribe({
          next: finalAudit => {
            this.isSavingCounts = false;
            this.closeCountModal();
            this.toasterService.success(`Audit ${finalAudit.auditNumber} submitted for Managerial Review!`);
            this.fetchAudits();
            this.fetchMetrics();
          },
          error: err => {
            this.isSavingCounts = false;
            this.toasterService.error(err.error?.message || 'Failed to submit count.');
            this.cdr.detectChanges();
          },
        });
      },
      error: err => {
        this.isSavingCounts = false;
        this.toasterService.error(err.error?.message || 'Failed to save counts before submission.');
        this.cdr.detectChanges();
      },
    });
  }

  // --- RECONCILE / REVIEW MODAL ---
  openReconcileModal(audit: CycleCountAuditDto): void {
    this.activeAuditForReconcile = audit;
    this.approverName = audit.assignedAuditorName ? 'Lead Inventory Controller' : 'Warehouse Facility Manager';
    this.approvalNotes = '';
    this.isReconcileModalOpen = true;
  }

  closeReconcileModal(): void {
    this.isReconcileModalOpen = false;
    this.activeAuditForReconcile = null;
  }

  requestReCount(): void {
    if (!this.activeAuditForReconcile) return;

    this.confirmationService
      .warn('Are you sure you want to request a re-count on items with variances?', 'Request Blind Re-Count')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.auditService.requestReCount(this.activeAuditForReconcile!.id!, this.approvalNotes).subscribe({
            next: res => {
              this.closeReconcileModal();
              this.toasterService.info(`Re-count requested for audit ${res.auditNumber}.`);
              this.fetchAudits();
              this.fetchMetrics();
            },
            error: err => {
              this.toasterService.error(err.error?.message || 'Failed to request re-count.');
            },
          });
        }
      });
  }

  approveAndReconcile(): void {
    if (!this.activeAuditForReconcile) return;

    this.confirmationService
      .success(
        `This will automatically post inventory adjustments (+/-) to system stock and finalize Audit ${this.activeAuditForReconcile.auditNumber}. Confirm approval?`,
        'Authorize Stock Reconciliation'
      )
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.isReconciling = true;
          this.auditService
            .approveAndReconcile(this.activeAuditForReconcile!.id!, {
              approverName: this.approverName,
              approvalNotes: this.approvalNotes,
            })
            .subscribe({
              next: res => {
                this.isReconciling = false;
                this.closeReconcileModal();
                this.toasterService.success(`Audit ${res.auditNumber} successfully approved and reconciled! All inventory balances updated.`);
                this.fetchAudits();
                this.fetchMetrics();
              },
              error: err => {
                this.isReconciling = false;
                this.toasterService.error(err.error?.message || 'Failed to reconcile audit.');
                this.cdr.detectChanges();
              },
            });
        }
      });
  }

  cancelAudit(audit: CycleCountAuditDto): void {
    this.confirmationService
      .warn(`Are you sure you want to cancel audit ${audit.auditNumber}?`, 'Cancel Audit Order')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.auditService.cancelAudit(audit.id!, 'Cancelled by operator').subscribe({
            next: () => {
              this.toasterService.info(`Audit ${audit.auditNumber} marked as cancelled.`);
              this.fetchAudits();
              this.fetchMetrics();
            },
            error: err => {
              this.toasterService.error(err.error?.message || 'Failed to cancel audit.');
            },
          });
        }
      });
  }

  // --- PRINTABLE COUNT SHEET ---
  openPrintModal(audit: CycleCountAuditDto): void {
    this.activeAuditForPrint = audit;
    this.isPrintModalOpen = true;
  }

  closePrintModal(): void {
    this.isPrintModalOpen = false;
    this.activeAuditForPrint = null;
  }

  printDocument(): void {
    window.print();
  }

  // Helpers
  getTypeLabel(type: CycleCountType): string {
    switch (type) {
      case CycleCountType.ABCCategory:
        return 'ABC Velocity Audit';
      case CycleCountType.ZoneLocationSweep:
        return 'Zone Location Sweep';
      case CycleCountType.ProductSpecific:
        return 'SKU Spot Check';
      case CycleCountType.RandomSampling:
        return 'Random Sampling';
      case CycleCountType.FullWallToWall:
        return 'Wall-to-Wall Physical Count';
      default:
        return 'Cycle Count';
    }
  }

  getStatusClass(status: CycleCountStatus): string {
    switch (status) {
      case CycleCountStatus.Draft:
        return 'status-draft';
      case CycleCountStatus.InProgress:
        return 'status-inprogress';
      case CycleCountStatus.UnderReview:
        return 'status-review';
      case CycleCountStatus.ReCountRequested:
        return 'status-recount';
      case CycleCountStatus.Reconciled:
        return 'status-reconciled';
      case CycleCountStatus.Cancelled:
        return 'status-cancelled';
      default:
        return 'status-draft';
    }
  }

  getStatusLabel(status: CycleCountStatus): string {
    switch (status) {
      case CycleCountStatus.Draft:
        return 'Draft Planned';
      case CycleCountStatus.InProgress:
        return 'Counting in Progress';
      case CycleCountStatus.UnderReview:
        return 'Under Manager Review';
      case CycleCountStatus.ReCountRequested:
        return 'Re-Count Requested';
      case CycleCountStatus.Reconciled:
        return 'Reconciled & Posted';
      case CycleCountStatus.Cancelled:
        return 'Cancelled';
      default:
        return 'Unknown';
    }
  }

  getReasonLabel(reason: DiscrepancyReason): string {
    const found = this.discrepancyReasons.find(r => r.value === reason);
    return found ? found.label : 'None';
  }

  calculateLiveVariance(item: CycleCountAuditItemDto): number {
    const draft = this.countItemsDraft[item.id];
    if (!draft || draft.countedQuantity === undefined || draft.countedQuantity === null) {
      return 0;
    }
    return Number(draft.countedQuantity) - item.systemQuantity;
  }
}
