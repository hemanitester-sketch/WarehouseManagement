import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { PurchaseReceiveService, PurchaseReceiveRecord, PurchaseReceiveItemRecord } from '../services/purchase-receive.service';
import { InboundOrderService } from '../../proxy/inbound/inbound-order.service';
import { InboundOrderDto, InboundOrderLineDto, ReceiveInboundGoodsDto } from '../../proxy/inbound/models';
import { WarehouseService, WarehouseLocationService } from '../../proxy/warehouses';
import { WarehouseDto, WarehouseLocationDto } from '../../proxy/warehouses/models';
import { ProductService } from '../../proxy/products/product.service';
import { ProductDto } from '../../proxy/products/models';

@Component({
  selector: 'app-purchase-receives',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './purchase-receives.component.html',
  styleUrls: ['./purchase-receives.component.scss']
})
export class PurchaseReceivesComponent implements OnInit {
  private receiveService = inject(PurchaseReceiveService);
  private inboundService = inject(InboundOrderService);
  private warehouseService = inject(WarehouseService);
  private locationService = inject(WarehouseLocationService);
  private productService = inject(ProductService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  receives: PurchaseReceiveRecord[] = [];
  openOrders: InboundOrderDto[] = [];
  warehouses: WarehouseDto[] = [];
  locations: WarehouseLocationDto[] = [];
  products: ProductDto[] = [];

  Math = Math;

  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedWarehouseFilter = '';

  // Pagination
  currentPage = 1;
  pageSize = 50;

  // Modals
  isCreateReceiveModalOpen = false;
  selectedReceiveForView: PurchaseReceiveRecord | null = null;
  isViewGrnModalOpen = false;

  // New Receive Form State
  selectedOrderForReceive: InboundOrderDto | null = null;
  selectedOrderId: string = '';
  receiveCarrierName = 'BlueDart Logistics';
  receiveVehicleNumber = 'DL-01-AX-4412';
  receiveGatePassNumber = '';
  receiveNotes = 'Dock shipment arrived, physical packaging intact.';
  inspectorName = 'Dock Officer - Inward Team';

  // Line items state for receive
  receiveLinesState: {
    line: InboundOrderLineDto;
    receivingQty: number;
    acceptedQty: number;
    damagedQty: number;
    rejectionReason: string;
    destinationLocationId: string;
    lotBatchNumber: string;
    serialNumber: string;
    expiryDate: string;
  }[] = [];

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadLocations();
    this.loadReceives();
    this.loadOpenOrders();
  }

  loadWarehouses(): void {
    this.warehouseService.getList({ maxResultCount: 50, skipCount: 0 }).subscribe({
      next: (res) => {
        this.warehouses = res.items || [];
        this.cdr.detectChanges();
      }
    });
  }

  loadLocations(): void {
    this.locationService.getList({ maxResultCount: 300, skipCount: 0 }).subscribe({
      next: (res) => {
        this.locations = res.items || [];
        this.cdr.detectChanges();
      }
    });
  }

  loadOpenOrders(): void {
    this.inboundService.getList({ maxResultCount: 100, skipCount: 0 }).subscribe({
      next: (res) => {
        const all = res.items || [];
        // Orders with pending quantities
        this.openOrders = all.filter(o => (o.totalReceivedQuantity || 0) < (o.totalExpectedQuantity || 1));
        this.cdr.detectChanges();
      }
    });
  }

  loadReceives(): void {
    this.isLoading = true;
    this.receiveService.getReceives().subscribe({
      next: (list) => {
        this.receives = list || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Filtered & Paginated
  get filteredReceives(): PurchaseReceiveRecord[] {
    let list = this.receives;

    if (this.selectedWarehouseFilter) {
      list = list.filter(r => r.warehouseId === this.selectedWarehouseFilter);
    }

    if (this.searchQuery && this.searchQuery.trim()) {
      const q = this.searchQuery.trim().toLowerCase();
      list = list.filter(r => 
        (r.receiveNumber && r.receiveNumber.toLowerCase().includes(q)) ||
        (r.orderNumber && r.orderNumber.toLowerCase().includes(q)) ||
        (r.supplierName && r.supplierName.toLowerCase().includes(q)) ||
        (r.carrierName && r.carrierName.toLowerCase().includes(q))
      );
    }

    return list;
  }

  get paginatedReceives(): PurchaseReceiveRecord[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredReceives.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredReceives.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    if (this.filteredReceives.length === 0) return 0;
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredReceives.length);
  }

  onPageSizeChange(newSize: any): void {
    this.pageSize = Number(newSize);
    this.currentPage = 1;
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

  // KPI Metrics
  get totalGrnCount(): number {
    return this.receives.length;
  }

  get totalUnitsReceived(): number {
    return this.receives.reduce((sum, r) => sum + (r.totalReceivedQuantity || 0), 0);
  }

  get totalUnitsAccepted(): number {
    return this.receives.reduce((sum, r) => sum + (r.totalAcceptedQuantity || 0), 0);
  }

  get totalUnitsDamaged(): number {
    return this.receives.reduce((sum, r) => sum + (r.totalDamagedQuantity || 0), 0);
  }

  // Modal: Open New Receive
  openNewReceiveModal(preselectedPoId?: string): void {
    this.selectedOrderId = preselectedPoId || (this.openOrders.length > 0 ? this.openOrders[0].id! : '');
    const rand = Math.floor(1000 + Math.random() * 9000);
    this.receiveGatePassNumber = `GP-${new Date().getFullYear()}-${rand}`;
    this.receiveCarrierName = 'BlueDart Logistics';
    this.receiveVehicleNumber = 'DL-01-AX-4412';
    this.receiveNotes = 'Dock shipment arrived, physical packaging intact.';
    this.inspectorName = 'Dock Officer - Inward Team';
    this.onOrderSelectionChange();
    this.isCreateReceiveModalOpen = true;
  }

  onOrderSelectionChange(): void {
    if (!this.selectedOrderId) {
      this.selectedOrderForReceive = null;
      this.receiveLinesState = [];
      return;
    }

    this.inboundService.get(this.selectedOrderId).subscribe({
      next: (order) => {
        this.selectedOrderForReceive = order;
        const whLocations = this.locations.filter(l => l.warehouseId === order.warehouseId);
        const defaultLocId = whLocations.length > 0 ? whLocations[0].id! : '';
        const defaultExpiry = new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

        this.receiveLinesState = (order.lines || []).map(line => {
          const remaining = Math.max(0, (line.expectedQuantity || 0) - (line.receivedQuantity || 0));
          return {
            line,
            receivingQty: remaining,
            acceptedQty: remaining,
            damagedQty: 0,
            rejectionReason: '',
            destinationLocationId: defaultLocId,
            lotBatchNumber: 'LOT-2026-REC',
            serialNumber: '',
            expiryDate: defaultExpiry
          };
        });

        this.cdr.detectChanges();
      }
    });
  }

  onReceivingQtyChanged(item: any): void {
    if (item.receivingQty < 0) item.receivingQty = 0;
    // auto update accepted to receivingQty - damagedQty
    item.acceptedQty = Math.max(0, item.receivingQty - (item.damagedQty || 0));
  }

  onDamagedQtyChanged(item: any): void {
    if (item.damagedQty < 0) item.damagedQty = 0;
    if (item.damagedQty > item.receivingQty) item.damagedQty = item.receivingQty;
    item.acceptedQty = Math.max(0, item.receivingQty - item.damagedQty);
  }

  submitReceive(): void {
    if (!this.selectedOrderForReceive) {
      this.toasterService.warn('Please select a purchase order.');
      return;
    }

    const linesToReceive = this.receiveLinesState.filter(s => s.receivingQty > 0);
    if (linesToReceive.length === 0) {
      this.toasterService.warn('Please enter receiving quantity for at least one item.');
      return;
    }

    // Validate location for each line
    for (const item of linesToReceive) {
      if (!item.destinationLocationId) {
        this.toasterService.warn(`Please select a destination bin location for ${item.line.productSku || item.line.productName}.`);
        return;
      }
    }

    this.isSaving = true;

    // Process each line through inboundService.receiveGoods
    const receivePromises = linesToReceive.map(item => {
      const receiveDto: ReceiveInboundGoodsDto = {
        inboundOrderId: this.selectedOrderForReceive!.id!,
        inboundOrderLineId: item.line.id!,
        receivedQuantity: item.receivingQty,
        destinationLocationId: item.destinationLocationId,
        lotBatchNumber: item.lotBatchNumber || 'LOT-2026',
        serialNumber: item.serialNumber || undefined,
        expiryDate: item.expiryDate || undefined,
        notes: this.receiveNotes
      };
      return this.inboundService.receiveGoods(receiveDto).toPromise();
    });

    Promise.all(receivePromises).then(() => {
      // Build PurchaseReceive record
      const itemsRecord: PurchaseReceiveItemRecord[] = linesToReceive.map(item => {
        const loc = this.locations.find(l => l.id === item.destinationLocationId);
        return {
          productId: item.line.productId || '',
          productSku: item.line.productSku || 'SKU',
          productName: item.line.productName || 'Product',
          productVariantId: item.line.productVariantId,
          variantSku: item.line.variantSku,
          variantName: item.line.variantName,
          quantityOrdered: item.line.expectedQuantity || 0,
          quantityReceived: item.receivingQty,
          quantityAccepted: item.acceptedQty,
          quantityDamaged: item.damagedQty,
          rejectionReason: item.damagedQty > 0 ? (item.rejectionReason || 'Damaged packaging') : undefined,
          destinationLocationId: item.destinationLocationId,
          destinationLocationCode: loc?.barcode || 'BIN-LOC',
          lotBatchNumber: item.lotBatchNumber,
          serialNumber: item.serialNumber,
          expiryDate: item.expiryDate,
          unitPrice: item.line.unitPrice || 0
        };
      });

      const totalRec = itemsRecord.reduce((sum, i) => sum + i.quantityReceived, 0);
      const totalAcc = itemsRecord.reduce((sum, i) => sum + i.quantityAccepted, 0);
      const totalDam = itemsRecord.reduce((sum, i) => sum + i.quantityDamaged, 0);

      const createdRecord = this.receiveService.recordReceive({
        inboundOrderId: this.selectedOrderForReceive!.id || '',
        orderNumber: this.selectedOrderForReceive!.orderNumber || 'PO-ORDER',
        supplierName: this.selectedOrderForReceive!.supplierName || 'Supplier',
        warehouseId: this.selectedOrderForReceive!.warehouseId || '',
        warehouseName: this.selectedOrderForReceive!.warehouseName || 'Main Hub',
        warehouseCode: this.selectedOrderForReceive!.warehouseCode || 'WH',
        supplierInvoiceNumber: this.selectedOrderForReceive!.supplierInvoiceNumber || 'INV-REC',
        carrierName: this.receiveCarrierName,
        vehicleNumber: this.receiveVehicleNumber,
        gatePassNumber: this.receiveGatePassNumber,
        receivedDate: new Date().toISOString(),
        status: 'Completed',
        totalOrderedQuantity: this.selectedOrderForReceive!.totalExpectedQuantity || 0,
        totalReceivedQuantity: totalRec,
        totalAcceptedQuantity: totalAcc,
        totalDamagedQuantity: totalDam,
        notes: this.receiveNotes,
        inspectedBy: this.inspectorName,
        items: itemsRecord
      });

      this.isSaving = false;
      this.isCreateReceiveModalOpen = false;
      this.toasterService.success(`Goods received successfully! GRN generated: ${createdRecord.receiveNumber}`, 'GRN Confirmed');
      this.loadReceives();
      this.loadOpenOrders();

      // Open GRN view immediately
      this.viewGrn(createdRecord);
    }).catch(err => {
      this.isSaving = false;
      this.toasterService.error(err?.error?.message || 'Failed to complete goods receiving');
      this.cdr.detectChanges();
    });
  }

  viewGrn(record: PurchaseReceiveRecord): void {
    this.selectedReceiveForView = record;
    this.isViewGrnModalOpen = true;
  }

  printGrn(): void {
    window.print();
  }

  getWarehouseLocations(warehouseId: string): WarehouseLocationDto[] {
    return this.locations.filter(l => l.warehouseId === warehouseId);
  }
}
