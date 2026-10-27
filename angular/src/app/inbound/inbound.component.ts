import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { InboundOrderService } from '../proxy/inbound';
import { 
  InboundOrderDto, 
  InboundOrderLineDto, 
  ReceiveInboundGoodsDto, 
  CreateUpdateInboundOrderDto 
} from '../proxy/inbound/models';
import { InboundOrderStatus, inboundOrderStatusOptions } from '../proxy/inbound/inbound-order-status.enum';
import { WarehouseService, WarehouseLocationService } from '../proxy/warehouses';
import { ProductService } from '../proxy/products';
import { WarehouseDto, WarehouseLocationDto } from '../proxy/warehouses/models';
import { ProductDto } from '../proxy/products/models';

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
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  orders: InboundOrderDto[] = [];
  warehouses: WarehouseDto[] = [];
  products: ProductDto[] = [];
  locations: WarehouseLocationDto[] = [];

  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedOrder: InboundOrderDto | null = null;

  // Receive Putaway Modal
  isReceiveModalOpen = false;
  selectedLineForReceive: InboundOrderLineDto | null = null;
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
  newOrder: CreateUpdateInboundOrderDto = {
    orderNumber: `PO-${new Date().getFullYear()}-${Math.floor(1000 + Math.random() * 9000)}`,
    warehouseId: '',
    supplierName: '',
    supplierInvoiceNumber: '',
    status: InboundOrderStatus.Draft,
    notes: '',
    lines: []
  };

  selectedProductForLine: string = '';
  quantityForLine: number = 10;
  priceForLine: number = 0;

  statusOptions = inboundOrderStatusOptions;

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
    this.loadOrders();
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
    this.productService.getList({ maxResultCount: 100, skipCount: 0 }).subscribe({
      next: (res) => {
        this.products = res.items || [];
        if (this.products.length > 0) {
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
  }

  openReceiveModal(line: InboundOrderLineDto): void {
    if (!this.selectedOrder) return;
    this.selectedLineForReceive = line;
    const remaining = (line.expectedQuantity || 0) - (line.receivedQuantity || 0);
    this.receiveModel = {
      inboundOrderId: this.selectedOrder.id!,
      inboundOrderLineId: line.id!,
      receivedQuantity: remaining > 0 ? remaining : 1,
      destinationLocationId: '',
      lotBatchNumber: `LOT-${Date.now().toString().slice(-6)}`,
      serialNumber: '',
      expiryDate: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
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

  openCreateOrderModal(): void {
    const todayPlus2 = new Date(Date.now() + 2 * 24 * 60 * 60 * 1000);
    this.expectedArrivalDateStr = todayPlus2.toISOString().split('T')[0];

    this.newOrder = {
      orderNumber: `PO-${new Date().getFullYear()}-${Math.floor(1000 + Math.random() * 9000)}`,
      warehouseId: this.warehouses.length > 0 ? this.warehouses[0].id! : '',
      supplierName: '',
      supplierInvoiceNumber: '',
      status: InboundOrderStatus.Draft,
      notes: '',
      lines: []
    };

    if (this.products.length > 0) {
      this.selectedProductForLine = this.products[0].id!;
      this.onProductSelected();
    }
    this.quantityForLine = 10;
    this.isCreateOrderModalOpen = true;
  }

  onProductSelected(): void {
    const prod = this.products.find(p => p.id === this.selectedProductForLine);
    if (prod && prod.unitPrice != null && prod.unitPrice > 0) {
      this.priceForLine = prod.unitPrice;
    } else {
      this.priceForLine = 0;
    }
  }

  addLineToOrder(): void {
    if (!this.selectedProductForLine) {
      this.toasterService.warn('Please select a product.');
      return;
    }
    if (this.quantityForLine <= 0) {
      this.toasterService.warn('Quantity must be greater than 0.');
      return;
    }

    this.newOrder.lines = this.newOrder.lines || [];

    // Check if line already exists for this product
    const existingIndex = this.newOrder.lines.findIndex(l => l.productId === this.selectedProductForLine);
    if (existingIndex >= 0) {
      this.newOrder.lines[existingIndex].expectedQuantity += this.quantityForLine;
      if (this.priceForLine > 0) {
        this.newOrder.lines[existingIndex].unitPrice = this.priceForLine;
      }
      this.toasterService.info('Updated quantity for existing line item.');
    } else {
      this.newOrder.lines.push({
        productId: this.selectedProductForLine,
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
