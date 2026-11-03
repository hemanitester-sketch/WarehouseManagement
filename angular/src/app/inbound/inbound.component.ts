import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { InboundOrderService } from '../proxy/inbound';
import { 
  InboundOrderDto, 
  InboundOrderLineDto, 
  ReceiveInboundGoodsDto, 
  CreateUpdateInboundOrderDto,
  CreateInboundOrderLineDto 
} from '../proxy/inbound/models';
import { InboundOrderStatus, inboundOrderStatusOptions } from '../proxy/inbound/inbound-order-status.enum';
import { WarehouseService, WarehouseLocationService } from '../proxy/warehouses';
import { ProductService } from '../proxy/products';
import { WarehouseDto, WarehouseLocationDto } from '../proxy/warehouses/models';
import { ProductDto, ProductVariantDto, ProductType } from '../proxy/products/models';
import { ProductVariantService } from '../proxy/products/product-variant.service';
import { BarcodeLookupService } from '../proxy/barcodes/barcode-lookup.service';
import { SupplierService } from '../proxy/suppliers/supplier.service';
import { SupplierLookupDto } from '../proxy/suppliers/models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-inbound',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inbound.component.html',
  styleUrls: ['./inbound.component.scss']
})
export class InboundComponent implements OnInit {
  private inboundService = inject(InboundOrderService);
  private warehouseService = inject(WarehouseService);
  private locationService = inject(WarehouseLocationService);
  private productService = inject(ProductService);
  private productVariantService = inject(ProductVariantService);
  private barcodeLookupService = inject(BarcodeLookupService);
  private supplierService = inject(SupplierService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);

  ProductType = ProductType;

  orders: InboundOrderDto[] = [];
  warehouses: WarehouseDto[] = [];
  products: ProductDto[] = [];
  locations: WarehouseLocationDto[] = [];
  suppliers: SupplierLookupDto[] = [];

  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedOrder: InboundOrderDto | null = null;

  // Dock Barcode Scanner for Rapid Receiving
  quickReceiveBarcode: string = '';
  isScanningBarcode = false;

  // Receive Putaway Modal
  isReceiveModalOpen = false;
  selectedLineForReceive: InboundOrderLineDto | null = null;
  isExpiryRequired = false;
  isLotRequired = false;
  isSerialRequired = false;
  receiveModel: ReceiveInboundGoodsDto = {
    inboundOrderId: '',
    inboundOrderLineId: '',
    receivedQuantity: 0,
    destinationLocationId: '',
    lotBatchNumber: 'LOT-2026-REC',
    serialNumber: '',
    expiryDate: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    notes: 'Inbound Dock Receiving'
  };

  // Create Order Modal
  isCreateOrderModalOpen = false;
  expectedArrivalDateStr: string = '';
  isOrderNumberDuplicate = false;
  orderNumberTouched = false;
  isCheckingOrderNumber = false;
  private orderNumberCheckTimer: any = null;

  newOrder: CreateUpdateInboundOrderDto = {
    orderNumber: '',
    warehouseId: '',
    supplierName: '',
    supplierInvoiceNumber: '',
    status: InboundOrderStatus.Draft,
    notes: '',
    lines: []
  };

  selectedProductForLine: string = '';
  selectedVariantForLine: string = '';
  availableVariantsForSelectedProduct: ProductVariantDto[] = [];
  isLoadingVariants = false;
  quantityForLine: number = 10;
  priceForLine: number = 0;
  loadedVariantsMap = new Map<string, ProductVariantDto>();

  statusOptions = inboundOrderStatusOptions;

  // Pagination State (50 Limit default)
  currentPage = 1;
  pageSize = 50;

  get paginatedOrders(): InboundOrderDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.orders.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.orders.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    return this.orders.length === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.orders.length);
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

  // KPIs
  get totalOrdersCount(): number {
    return this.orders.length;
  }

  get pendingOrdersCount(): number {
    return this.orders.filter(o => 
      o.status === InboundOrderStatus.Draft || 
      o.status === InboundOrderStatus.InTransit || 
      o.status === InboundOrderStatus.Arrived
    ).length;
  }

  get partiallyReceivedCount(): number {
    return this.orders.filter(o => o.status === InboundOrderStatus.PartiallyReceived).length;
  }

  get completedOrdersCount(): number {
    return this.orders.filter(o => o.status === InboundOrderStatus.Completed).length;
  }

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadProducts();
    this.loadSuppliers();
    this.loadOrders();
  }

  loadSuppliers(): void {
    this.supplierService.getLookup().subscribe({
      next: (res) => {
        this.suppliers = res.items || [];
        this.cdr.detectChanges();
      }
    });
  }

  navigateToSuppliers(): void {
    this.router.navigate(['/suppliers']);
  }

  loadWarehouses(): void {
    this.warehouseService.getList({ maxResultCount: 50, skipCount: 0 }).subscribe({
      next: (res) => {
        this.warehouses = res.items || [];
        if (this.warehouses.length > 0 && !this.newOrder.warehouseId) {
          this.newOrder.warehouseId = this.warehouses[0].id!;
        }
        this.cdr.detectChanges();
      }
    });
  }

  loadProducts(): void {
    this.productService.getList({ maxResultCount: 200, skipCount: 0 }).subscribe({
      next: (res) => {
        this.products = res.items || [];
        if (this.products.length > 0 && !this.selectedProductForLine) {
          this.selectedProductForLine = this.products[0].id!;
          this.onProductSelected();
        }
        this.cdr.detectChanges();
      }
    });
  }

  loadOrders(): void {
    this.isLoading = true;
    this.inboundService.getList({
      filter: this.searchQuery?.trim() || undefined,
      maxResultCount: 100,
      skipCount: 0
    }).subscribe({
      next: (res) => {
        this.orders = res.items || [];
        this.currentPage = 1;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.toasterService.error(err?.error?.message || 'Failed to load inbound orders');
        this.cdr.detectChanges();
      }
    });
  }

  getStatusBadge(status: InboundOrderStatus | undefined): string {
    switch (status) {
      case InboundOrderStatus.Completed: return 'wms-badge-success';
      case InboundOrderStatus.PartiallyReceived: return 'wms-badge-info';
      case InboundOrderStatus.Arrived: return 'wms-badge-warning';
      case InboundOrderStatus.InTransit: return 'wms-badge-primary';
      case InboundOrderStatus.Cancelled: return 'wms-badge-danger';
      default: return 'wms-badge-secondary';
    }
  }

  getStatusLabel(status: InboundOrderStatus | undefined): string {
    const found = this.statusOptions.find(s => s.value === status);
    return found ? found.key : 'Draft';
  }

  viewOrderDetails(order: InboundOrderDto): void {
    this.selectedOrder = order;
    this.quickReceiveBarcode = '';
  }

  onDockScanBarcode(): void {
    const barcode = (this.quickReceiveBarcode || '').trim();
    if (!barcode) {
      this.toasterService.warn('Please enter or scan a barcode.');
      return;
    }
    if (!this.selectedOrder || !this.selectedOrder.lines || this.selectedOrder.lines.length === 0) {
      this.toasterService.warn('No order lines available to receive.');
      return;
    }

    // 1. Direct match against order lines by productSku or variantSku
    const directMatch = this.selectedOrder.lines.find(l => 
      !l.isCompleted && (
        (l.variantSku && l.variantSku.toUpperCase() === barcode.toUpperCase()) ||
        (l.productSku && l.productSku.toUpperCase() === barcode.toUpperCase())
      )
    );

    if (directMatch) {
      this.openReceiveModal(directMatch);
      this.toasterService.success(`Line matched for SKU: ${directMatch.variantSku || directMatch.productSku}`, 'Barcode Match');
      this.quickReceiveBarcode = '';
      return;
    }

    // 2. Enterprise Barcode Resolution (covers Supplier Barcode Mappings & Pack Levels)
    this.isScanningBarcode = true;
    this.barcodeLookupService.resolveBarcode({
      barcodeValue: barcode,
      warehouseId: this.selectedOrder.warehouseId
    }).subscribe({
      next: (res) => {
        this.isScanningBarcode = false;
        if (!res || !res.isFound) {
          this.toasterService.error(`Barcode '${barcode}' was not recognized in the system.`, 'Barcode Not Found');
          this.cdr.detectChanges();
          return;
        }

        const matchedLine = this.selectedOrder!.lines!.find(l => {
          if (l.isCompleted) return false;
          if (res.productVariantId && l.productVariantId === res.productVariantId) return true;
          if (!res.productVariantId && l.productId === res.productId) return true;
          if (res.sku && ((l.variantSku && l.variantSku.toUpperCase() === res.sku.toUpperCase()) || (l.productSku && l.productSku.toUpperCase() === res.sku.toUpperCase()))) return true;
          return false;
        });

        if (matchedLine) {
          this.openReceiveModal(matchedLine);
          if (res.unitsPerPackage && res.unitsPerPackage > 0) {
            const remaining = (matchedLine.expectedQuantity || 0) - (matchedLine.receivedQuantity || 0);
            this.receiveModel.receivedQuantity = Math.min(res.unitsPerPackage, remaining > 0 ? remaining : res.unitsPerPackage);
          }
          if (res.lotBatchNumber) {
            this.receiveModel.lotBatchNumber = res.lotBatchNumber;
          }
          if (res.expirationDate) {
            this.receiveModel.expiryDate = res.expirationDate.split('T')[0];
          }
          const matchLabel = res.isSupplierBarcode 
            ? `Supplier Barcode (${res.supplierProductCode || barcode}) mapped to ${res.variantName || res.productName || res.sku}`
            : (res.variantName ? `${res.productName} (${res.variantName})` : (res.productName || res.sku || 'Item'));
          this.toasterService.success(`Identified: ${matchLabel}`, 'Scan Match Confirmed');
          this.quickReceiveBarcode = '';
        } else {
          const prodDesc = res.variantName ? `${res.productName} (${res.variantName})` : (res.productName || res.sku);
          this.toasterService.warn(`Scanned item '${prodDesc}' was recognized, but is not an open line in PO ${this.selectedOrder!.orderNumber}.`, 'Item Not in PO');
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isScanningBarcode = false;
        this.toasterService.error(err?.error?.message || 'Failed to resolve scanned barcode');
        this.cdr.detectChanges();
      }
    });
  }

  openReceiveModal(line: InboundOrderLineDto): void {
    if (!this.selectedOrder) return;
    this.selectedLineForReceive = line;
    const remaining = (line.expectedQuantity || 0) - (line.receivedQuantity || 0);

    const prod = this.products.find(p => p.id === line.productId);
    this.isExpiryRequired = !!prod?.isExpiryTracked;
    this.isLotRequired = !!prod?.isLotTracked;
    this.isSerialRequired = !!prod?.isSerialTracked;

    this.receiveModel = {
      inboundOrderId: this.selectedOrder.id!,
      inboundOrderLineId: line.id!,
      receivedQuantity: remaining > 0 ? remaining : 1,
      destinationLocationId: '',
      lotBatchNumber: `LOT-${Date.now().toString().slice(-6)}`,
      serialNumber: '',
      expiryDate: this.isExpiryRequired 
        ? new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0] 
        : '',
      notes: 'Quality checked & putaway'
    };

    // Load available locations for target warehouse
    this.locationService.getList({ warehouseId: this.selectedOrder.warehouseId!, maxResultCount: 100, skipCount: 0 }).subscribe({
      next: (res) => {
        this.locations = res.items || [];
        if (this.locations.length > 0) {
          this.receiveModel.destinationLocationId = this.locations[0].id!;
        }
        this.isReceiveModalOpen = true;
        this.cdr.detectChanges();
      }
    });
  }

  executeReceive(): void {
    if (!this.receiveModel.destinationLocationId) {
      this.toasterService.warn('Please select a destination bin location.');
      return;
    }
    if (this.receiveModel.receivedQuantity <= 0) {
      this.toasterService.warn('Received quantity must be greater than zero.');
      return;
    }
    if (this.isExpiryRequired && !this.receiveModel.expiryDate) {
      this.toasterService.warn('Expiry date is mandatory for this FEFO expiry-tracked product.', 'Expiry Date Required');
      return;
    }
    if (this.isLotRequired && (!this.receiveModel.lotBatchNumber || !this.receiveModel.lotBatchNumber.trim())) {
      this.toasterService.warn('Lot / Batch number is required for lot-tracked inventory.', 'Lot Number Required');
      return;
    }
    if (this.isSerialRequired && (!this.receiveModel.serialNumber || !this.receiveModel.serialNumber.trim())) {
      this.toasterService.warn('Serial number is required for serialized inventory.', 'Serial Number Required');
      return;
    }

    this.isSaving = true;
    this.inboundService.receiveGoods(this.receiveModel).subscribe({
      next: () => {
        this.isSaving = false;
        this.isReceiveModalOpen = false;
        this.toasterService.success('Goods received & stock deposited successfully into bin!', 'Put-away Confirmed');
        this.loadOrders();
        // Refresh selected order
        if (this.selectedOrder) {
          this.inboundService.get(this.selectedOrder.id!).subscribe({
            next: (ord) => {
              this.selectedOrder = ord;
              this.cdr.detectChanges();
            }
          });
        }
      },
      error: (err) => {
        this.isSaving = false;
        this.toasterService.error(err?.error?.message || 'Failed to receive goods');
        this.cdr.detectChanges();
      }
    });
  }

  generateUniquePoNumber(): string {
    const year = new Date().getFullYear();
    let candidate = '';
    let exists = true;
    let attempts = 0;
    while (exists && attempts < 50) {
      attempts++;
      const rand = Math.floor(1000 + Math.random() * 9000);
      candidate = `PO-${year}-${rand}`;
      exists = this.orders.some(o => (o.orderNumber || '').trim().toUpperCase() === candidate);
    }
    return candidate;
  }

  openCreateOrderModal(): void {
    this.loadSuppliers();
    const todayPlus2 = new Date(Date.now() + 2 * 24 * 60 * 60 * 1000);
    this.expectedArrivalDateStr = todayPlus2.toISOString().split('T')[0];

    const generatedPo = this.generateUniquePoNumber();
    this.newOrder = {
      orderNumber: generatedPo,
      warehouseId: this.warehouses.length > 0 ? this.warehouses[0].id! : '',
      supplierName: '',
      supplierInvoiceNumber: '',
      status: InboundOrderStatus.Draft,
      notes: '',
      lines: []
    };

    if (this.orderNumberCheckTimer) clearTimeout(this.orderNumberCheckTimer);
    this.isOrderNumberDuplicate = false;
    this.orderNumberTouched = true;
    this.isCheckingOrderNumber = false;

    this.selectedVariantForLine = '';
    this.availableVariantsForSelectedProduct = [];

    if (this.products.length > 0) {
      this.selectedProductForLine = this.products[0].id!;
      this.onProductSelected();
    }
    this.quantityForLine = 10;
    this.isCreateOrderModalOpen = true;
  }

  onOrderNumberInput(): void {
    this.orderNumberTouched = true;
    if (this.orderNumberCheckTimer) clearTimeout(this.orderNumberCheckTimer);

    const po = (this.newOrder.orderNumber || '').trim();
    if (!po) {
      this.isOrderNumberDuplicate = false;
      this.isCheckingOrderNumber = false;
      return;
    }

    this.isCheckingOrderNumber = true;
    this.orderNumberCheckTimer = setTimeout(() => {
      this.checkOrderNumberUniqueness();
      this.isCheckingOrderNumber = false;
      this.cdr.detectChanges();
    }, 300);
  }

  onOrderNumberBlur(): void {
    if (this.orderNumberCheckTimer) clearTimeout(this.orderNumberCheckTimer);
    this.checkOrderNumberUniqueness();
    this.isCheckingOrderNumber = false;
  }

  checkOrderNumberUniqueness(): void {
    this.orderNumberTouched = true;
    const po = (this.newOrder.orderNumber || '').trim().toUpperCase();
    if (!po) {
      this.isOrderNumberDuplicate = false;
      return;
    }
    this.isOrderNumberDuplicate = this.orders.some(o => (o.orderNumber || '').trim().toUpperCase() === po);
  }

  onProductSelected(): void {
    const prod = this.products.find(p => p.id === this.selectedProductForLine);
    this.selectedVariantForLine = '';
    this.availableVariantsForSelectedProduct = [];

    if (!prod) {
      this.priceForLine = 0;
      return;
    }

    const isVariantBased = prod.productType === ProductType.VariantBased || (prod.variantCount != null && prod.variantCount > 0);

    if (isVariantBased) {
      this.isLoadingVariants = true;
      this.productVariantService.getListByProductId(prod.id!).subscribe({
        next: (variants) => {
          this.isLoadingVariants = false;
          this.availableVariantsForSelectedProduct = variants || [];
          this.availableVariantsForSelectedProduct.forEach(v => {
            if (v.id) this.loadedVariantsMap.set(v.id, v);
          });

          if (this.availableVariantsForSelectedProduct.length > 0) {
            const def = this.availableVariantsForSelectedProduct.find(v => v.isDefault) || this.availableVariantsForSelectedProduct[0];
            this.selectedVariantForLine = def.id!;
            this.priceForLine = (def.purchasePrice != null && def.purchasePrice > 0)
              ? def.purchasePrice
              : ((def.sellingPrice != null && def.sellingPrice > 0) ? def.sellingPrice : (prod.unitPrice || 0));
          } else {
            this.priceForLine = prod.unitPrice || 0;
          }
          this.cdr.detectChanges();
        },
        error: () => {
          this.isLoadingVariants = false;
          this.priceForLine = prod.unitPrice || 0;
          this.cdr.detectChanges();
        }
      });
    } else {
      this.priceForLine = prod.unitPrice != null && prod.unitPrice > 0 ? prod.unitPrice : 0;
    }
  }

  onVariantSelected(): void {
    const v = this.availableVariantsForSelectedProduct.find(x => x.id === this.selectedVariantForLine);
    if (v) {
      this.priceForLine = (v.purchasePrice != null && v.purchasePrice > 0)
        ? v.purchasePrice
        : ((v.sellingPrice != null && v.sellingPrice > 0) ? v.sellingPrice : this.priceForLine);
    }
  }

  addLineToOrder(): void {
    if (!this.selectedProductForLine) {
      this.toasterService.warn('Please select a product.');
      return;
    }

    if (this.availableVariantsForSelectedProduct.length > 0 && !this.selectedVariantForLine) {
      this.toasterService.warn('Please select a specific variant.');
      return;
    }

    if (this.quantityForLine <= 0) {
      this.toasterService.warn('Quantity must be greater than 0.');
      return;
    }

    this.newOrder.lines = this.newOrder.lines || [];
    const targetVariantId = this.selectedVariantForLine || null;

    // Check if line already exists for this exact product + variant combination
    const existingIndex = this.newOrder.lines.findIndex(l => 
      l.productId === this.selectedProductForLine && 
      (l.productVariantId || null) === targetVariantId
    );

    if (existingIndex >= 0) {
      this.newOrder.lines[existingIndex].expectedQuantity += this.quantityForLine;
      if (this.priceForLine > 0) {
        this.newOrder.lines[existingIndex].unitPrice = this.priceForLine;
      }
      this.toasterService.info('Updated quantity for existing line item.');
    } else {
      this.newOrder.lines.push({
        productId: this.selectedProductForLine,
        productVariantId: targetVariantId,
        expectedQuantity: this.quantityForLine,
        unitPrice: this.priceForLine || 0
      });
    }

    // Reset default quantity for next item
    this.quantityForLine = 10;
    this.cdr.detectChanges();
  }

  removeLineFromOrder(index: number): void {
    if (this.newOrder.lines && this.newOrder.lines.length > index) {
      this.newOrder.lines.splice(index, 1);
      this.cdr.detectChanges();
    }
  }

  getLineSku(line: { productId?: string; productVariantId?: string | null }): string {
    if (line.productVariantId && this.loadedVariantsMap.has(line.productVariantId)) {
      return this.loadedVariantsMap.get(line.productVariantId)!.sku;
    }
    const p = this.products.find(x => x.id === line.productId);
    return (p && p.sku) ? p.sku : 'SKU';
  }

  getLineVariantName(line: { productVariantId?: string | null }): string | null {
    if (line.productVariantId && this.loadedVariantsMap.has(line.productVariantId)) {
      return this.loadedVariantsMap.get(line.productVariantId)!.variantName || null;
    }
    return null;
  }

  getProductSku(productId: string): string {
    const p = this.products.find(x => x.id === productId);
    return (p && p.sku) ? p.sku : 'SKU';
  }

  getProductName(productId: string): string {
    const p = this.products.find(x => x.id === productId);
    return (p && p.name) ? p.name : productId;
  }

  getOrderTotalQuantity(): number {
    if (!this.newOrder.lines) return 0;
    return this.newOrder.lines.reduce((sum, l) => sum + (l.expectedQuantity || 0), 0);
  }

  getOrderTotalAmount(): number {
    if (!this.newOrder.lines) return 0;
    return this.newOrder.lines.reduce((sum, l) => sum + ((l.expectedQuantity || 0) * (l.unitPrice || 0)), 0);
  }

  saveOrder(): void {
    if (!this.newOrder.orderNumber || !this.newOrder.orderNumber.trim()) {
      this.toasterService.warn('Please enter a valid PO Number.');
      return;
    }

    this.checkOrderNumberUniqueness();
    if (this.isOrderNumberDuplicate) {
      this.toasterService.error(`PO Number '${this.newOrder.orderNumber}' already exists in your records. Please enter a unique PO Number.`, 'Duplicate PO Number');
      return;
    }

    if (!this.newOrder.warehouseId) {
      this.toasterService.warn('Please select a destination facility/warehouse.');
      return;
    }
    if (!this.newOrder.supplierName || !this.newOrder.supplierName.trim()) {
      this.toasterService.warn('Please enter the Supplier Name.');
      return;
    }
    if (!this.newOrder.lines || this.newOrder.lines.length === 0) {
      this.toasterService.warn('Please add at least one product item to the purchase order before saving.');
      return;
    }

    this.newOrder.expectedArrivalDate = this.expectedArrivalDateStr 
      ? (new Date(this.expectedArrivalDateStr).toISOString() as any) 
      : (new Date().toISOString() as any);

    this.isSaving = true;
    this.inboundService.create(this.newOrder).subscribe({
      next: (created) => {
        this.isSaving = false;
        this.isCreateOrderModalOpen = false;
        this.toasterService.success(`Purchase Order ${created.orderNumber} created successfully!`, 'PO Created');
        this.loadOrders();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isSaving = false;
        this.toasterService.error(err?.error?.message || 'Failed to create Purchase Order');
        this.cdr.detectChanges();
      }
    });
  }
}
