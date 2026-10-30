import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { OutboundOrderService } from '../proxy/outbound';
import { 
  OutboundOrderDto, 
  OutboundOrderLineDto, 
  AllocateOrderStockDto, 
  PickOrderGoodsDto, 
  ShipOrderDto,
  CreateUpdateOutboundOrderDto
} from '../proxy/outbound/models';
import { OutboundOrderStatus, outboundOrderStatusOptions } from '../proxy/outbound/outbound-order-status.enum';
import { OrderPriority, orderPriorityOptions } from '../proxy/outbound/order-priority.enum';
import { WarehouseService } from '../proxy/warehouses';
import { InventoryService } from '../proxy/inventory';
import { ProductService } from '../proxy/products';
import { WarehouseDto } from '../proxy/warehouses/models';
import { InventoryItemDto } from '../proxy/inventory/models';
import { ProductDto } from '../proxy/products/models';
import { CustomerService } from '../proxy/customers';
import { CustomerDto } from '../proxy/customers/models';

@Component({
  selector: 'app-outbound',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './outbound.component.html',
  styleUrls: ['./outbound.component.scss']
})
export class OutboundComponent implements OnInit {
  private outboundService = inject(OutboundOrderService);
  private warehouseService = inject(WarehouseService);
  private inventoryService = inject(InventoryService);
  private productService = inject(ProductService);
  private customerService = inject(CustomerService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  orders: OutboundOrderDto[] = [];
  warehouses: WarehouseDto[] = [];
  products: ProductDto[] = [];
  customers: CustomerDto[] = [];
  availableStockItems: InventoryItemDto[] = [];

  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedOrder: OutboundOrderDto | null = null;
  selectedCustomerId: string = '';

  get selectedCustomer(): CustomerDto | undefined {
    return this.customers.find(c => c.id === this.selectedCustomerId);
  }

  // Pick Modal
  isPickModalOpen = false;
  selectedLineForPick: OutboundOrderLineDto | null = null;
  pickModel: PickOrderGoodsDto = {
    outboundOrderId: '',
    outboundOrderLineId: '',
    sourceInventoryItemId: '',
    pickedQuantity: 1,
    notes: 'Picked from bin'
  };

  // Ship Modal
  isShipModalOpen = false;
  shipModel: ShipOrderDto = {
    outboundOrderId: '',
    carrierName: 'BlueDart Express Logix',
    trackingNumber: `TRK-${Date.now().toString().slice(-8)}`
  };

  // Create Sales Order Modal
  isCreateOrderModalOpen = false;
  deliveryDateStr: string = '';
  newOrder: CreateUpdateOutboundOrderDto = {
    orderNumber: `SO-${new Date().getFullYear()}-${Math.floor(1000 + Math.random() * 9000)}`,
    warehouseId: '',
    customerName: '',
    shippingAddress: '',
    city: 'Mumbai',
    state: 'Maharashtra',
    postalCode: '400001',
    country: 'India',
    priority: OrderPriority.Normal,
    orderDate: new Date().toISOString(),
    requiredDeliveryDate: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
    notes: '',
    lines: []
  };

  selectedProductForLine: string = '';
  quantityForLine: number = 10;
  priceForLine: number = 0;

  statusOptions = outboundOrderStatusOptions;
  priorityOptions = orderPriorityOptions;

  // Pagination State (50 Limit default)
  currentPage = 1;
  pageSize = 50;

  get paginatedOrders(): OutboundOrderDto[] {
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

  // KPI Calculations
  get totalOrdersCount(): number {
    return this.orders.length;
  }

  get allocatedOrdersCount(): number {
    return this.orders.filter(o => 
      o.status === OutboundOrderStatus.Allocated || 
      o.status === OutboundOrderStatus.Picking
    ).length;
  }

  get pickedOrdersCount(): number {
    return this.orders.filter(o => o.status === OutboundOrderStatus.Picked).length;
  }

  get shippedOrdersCount(): number {
    return this.orders.filter(o => o.status === OutboundOrderStatus.Shipped).length;
  }

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadProducts();
    this.loadCustomers();
    this.loadOrders();
  }

  loadCustomers(): void {
    this.customerService.getList({ maxResultCount: 200, skipCount: 0 }).subscribe({
      next: (res) => {
        this.customers = res.items || [];
        this.cdr.detectChanges();
      }
    });
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
    this.outboundService.getList({
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
        this.toasterService.error(err?.error?.message || 'Failed to load sales orders');
        this.cdr.detectChanges();
      }
    });
  }

  getStatusBadge(status: OutboundOrderStatus | undefined): string {
    switch (status) {
      case OutboundOrderStatus.Shipped: return 'wms-badge-success';
      case OutboundOrderStatus.Picked: return 'wms-badge-info';
      case OutboundOrderStatus.Allocated: return 'wms-badge-warning';
      case OutboundOrderStatus.Picking: return 'wms-badge-primary';
      case OutboundOrderStatus.Cancelled: return 'wms-badge-danger';
      default: return 'wms-badge-secondary';
    }
  }

  getStatusLabel(status: OutboundOrderStatus | undefined): string {
    const found = this.statusOptions.find(s => s.value === status);
    return found ? found.key : 'Draft';
  }

  getPriorityBadge(priority: OrderPriority | undefined): string {
    switch (priority) {
      case OrderPriority.Urgent: return 'wms-badge-danger';
      case OrderPriority.High: return 'wms-badge-warning';
      default: return 'wms-badge-primary';
    }
  }

  getPriorityLabel(priority: OrderPriority | undefined): string {
    const found = this.priorityOptions.find(p => p.value === priority);
    return found ? found.key : 'Normal';
  }

  viewOrderDetails(ord: OutboundOrderDto): void {
    this.selectedOrder = ord;
  }

  canAllocateOrder(order: OutboundOrderDto | null | undefined): boolean {
    if (!order) return false;
    if (order.status === OutboundOrderStatus.Shipped || order.status === OutboundOrderStatus.Cancelled) {
      return false;
    }
    if (order.status === OutboundOrderStatus.Picked || order.status === OutboundOrderStatus.Packed) {
      return false;
    }
    const allocated = order.totalAllocatedQuantity || 0;
    const ordered = order.totalOrderedQuantity || 0;
    return allocated < ordered;
  }

  // Action 1: FEFO Stock Allocation
  allocateStock(order: OutboundOrderDto): void {
    this.outboundService.allocateStock({ outboundOrderId: order.id! }).subscribe({
      next: (updated) => {
        const allocated = updated?.totalAllocatedQuantity ?? 0;
        const ordered = updated?.totalOrderedQuantity ?? 0;

        if (allocated === 0) {
          this.toasterService.warn(
            `No stock available in warehouse to allocate for ${order.orderNumber}. Please receive stock via Inbound PO first.`,
            'Zero Stock Allocated'
          );
        } else if (allocated < ordered) {
          this.toasterService.info(
            `Partially allocated: ${allocated} of ${ordered} items allocated for ${order.orderNumber}. Additional stock needed to fulfill completely.`,
            'Partial Allocation'
          );
        } else {
          this.toasterService.success(
            `All ${allocated} items auto-allocated successfully for ${order.orderNumber} using FEFO! Ready for bin picking.`,
            'Stock Allocated'
          );
        }

        this.loadOrders();
        if (this.selectedOrder && this.selectedOrder.id === order.id) {
          this.selectedOrder = updated;
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        this.toasterService.error(err?.error?.message || 'Stock allocation failed. Insufficient available stock in warehouse.');
        this.cdr.detectChanges();
      }
    });
  }

  getSelectedStockItem(): InventoryItemDto | undefined {
    return this.availableStockItems.find(x => x.id === this.pickModel.sourceInventoryItemId);
  }

  getLineRemainingToPick(): number {
    if (!this.selectedLineForPick) return 0;
    return Math.max(0, (this.selectedLineForPick.allocatedQuantity || 0) - (this.selectedLineForPick.pickedQuantity || 0));
  }

  getMaxPickableQuantity(): number {
    const selectedItem = this.getSelectedStockItem();
    const stockOnHand = selectedItem?.quantityOnHand ?? 0;
    const remainingToPick = this.getLineRemainingToPick();
    return Math.min(stockOnHand, remainingToPick);
  }

  onSourceItemChange(): void {
    const max = this.getMaxPickableQuantity();
    if (this.pickModel.pickedQuantity > max) {
      this.pickModel.pickedQuantity = max > 0 ? max : 1;
    }
  }

  // Action 2: Picking Goods from Bin
  openPickModal(line: OutboundOrderLineDto): void {
    if (!this.selectedOrder) return;
    this.selectedLineForPick = line;
    const remainingToPick = Math.max(0, (line.allocatedQuantity || 0) - (line.pickedQuantity || 0));
    this.pickModel = {
      outboundOrderId: this.selectedOrder.id!,
      outboundOrderLineId: line.id!,
      sourceInventoryItemId: '',
      pickedQuantity: remainingToPick > 0 ? remainingToPick : 1,
      notes: 'Picked from bin'
    };

    // Load available inventory items for this product in this warehouse
    this.inventoryService.getList({
      warehouseId: this.selectedOrder.warehouseId!,
      productId: line.productId!,
      maxResultCount: 50,
      skipCount: 0
    }).subscribe({
      next: (res) => {
        this.availableStockItems = res.items || [];
        if (this.availableStockItems.length > 0) {
          this.pickModel.sourceInventoryItemId = this.availableStockItems[0].id!;
          const defaultPick = Math.min(this.availableStockItems[0].quantityOnHand || 0, remainingToPick);
          this.pickModel.pickedQuantity = defaultPick > 0 ? defaultPick : 1;
        }
        this.isPickModalOpen = true;
        this.cdr.detectChanges();
      }
    });
  }

  executePick(): void {
    if (!this.pickModel.sourceInventoryItemId) {
      this.toasterService.warn('Please select an inventory item/bin location.', 'Validation Warning');
      return;
    }
    if (!this.pickModel.pickedQuantity || this.pickModel.pickedQuantity <= 0) {
      this.toasterService.warn('Picked quantity must be greater than zero.', 'Validation Warning');
      return;
    }

    const selectedItem = this.getSelectedStockItem();
    const stockOnHand = selectedItem?.quantityOnHand ?? 0;
    if (this.pickModel.pickedQuantity > stockOnHand) {
      this.toasterService.warn(
        `Insufficient stock in source bin. Selected bin only has ${stockOnHand} units on hand, but you entered ${this.pickModel.pickedQuantity}.`,
        'Exceeds Bin Stock'
      );
      return;
    }

    const remainingNeeded = this.getLineRemainingToPick();
    if (remainingNeeded > 0 && this.pickModel.pickedQuantity > remainingNeeded) {
      this.toasterService.warn(
        `Picked quantity exceeds line requirement. Only ${remainingNeeded} units remaining to pick for this item (You entered ${this.pickModel.pickedQuantity}).`,
        'Exceeds Line Limit'
      );
      return;
    }

    this.isSaving = true;
    this.outboundService.pickGoods(this.pickModel).subscribe({
      next: () => {
        this.isSaving = false;
        this.isPickModalOpen = false;
        this.toasterService.success('Item successfully picked from bin!', 'Pick Confirmed');
        this.loadOrders();
        if (this.selectedOrder) {
          this.outboundService.get(this.selectedOrder.id!).subscribe({
            next: (ord) => {
              this.selectedOrder = ord;
              this.cdr.detectChanges();
            }
          });
        }
      },
      error: (err) => {
        this.isSaving = false;
        this.toasterService.error(err?.error?.message || 'Failed to confirm pick');
        this.cdr.detectChanges();
      }
    });
  }

  // Action 3: Ship / Dispatch Order
  openShipModal(order: OutboundOrderDto): void {
    this.shipModel = {
      outboundOrderId: order.id!,
      carrierName: 'Delhivery Express 3PL',
      trackingNumber: `DEL-EXP-${Date.now().toString().slice(-8)}`
    };
    this.isShipModalOpen = true;
  }

  executeShip(): void {
    if (!this.shipModel.carrierName || !this.shipModel.carrierName.trim()) {
      this.toasterService.warn('Please enter carrier/3PL logistics partner name.');
      return;
    }
    if (!this.shipModel.trackingNumber || !this.shipModel.trackingNumber.trim()) {
      this.toasterService.warn('Please enter tracking number / AWB.');
      return;
    }

    this.isSaving = true;
    this.outboundService.shipOrder(this.shipModel).subscribe({
      next: () => {
        this.isSaving = false;
        this.isShipModalOpen = false;
        this.toasterService.success('Order dispatched & marked as shipped successfully!', 'Dispatched');
        this.loadOrders();
        if (this.selectedOrder) {
          this.outboundService.get(this.selectedOrder.id!).subscribe({
            next: (ord) => {
              this.selectedOrder = ord;
              this.cdr.detectChanges();
            }
          });
        }
      },
      error: (err) => {
        this.isSaving = false;
        this.toasterService.error(err?.error?.message || 'Failed to dispatch order');
        this.cdr.detectChanges();
      }
    });
  }

  openCreateOrderModal(): void {
    const todayPlus2 = new Date(Date.now() + 2 * 24 * 60 * 60 * 1000);
    this.deliveryDateStr = todayPlus2.toISOString().split('T')[0];
    this.selectedCustomerId = '';
    this.loadCustomers();

    this.newOrder = {
      orderNumber: `SO-${new Date().getFullYear()}-${Math.floor(1000 + Math.random() * 9000)}`,
      warehouseId: this.warehouses.length > 0 ? this.warehouses[0].id! : '',
      customerName: '',
      shippingAddress: '',
      city: 'Mumbai',
      state: 'Maharashtra',
      postalCode: '400001',
      country: 'India',
      priority: OrderPriority.Normal,
      orderDate: new Date().toISOString(),
      requiredDeliveryDate: todayPlus2.toISOString(),
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

  onCustomerSelected(): void {
    const cust = this.customers.find(c => c.id === this.selectedCustomerId);
    if (cust) {
      this.newOrder.customerName = cust.name;
      if (cust.city) this.newOrder.city = cust.city;
      if (cust.state) this.newOrder.state = cust.state;
      if (cust.shippingAddress) this.newOrder.shippingAddress = cust.shippingAddress;
      if (cust.postalCode) this.newOrder.postalCode = cust.postalCode;
      if (cust.specialInstructions) {
        this.newOrder.notes = cust.specialInstructions;
      }
    } else {
      this.newOrder.customerName = '';
    }
    this.cdr.detectChanges();
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
      this.newOrder.lines[existingIndex].orderedQuantity += this.quantityForLine;
      if (this.priceForLine > 0) {
        this.newOrder.lines[existingIndex].unitPrice = this.priceForLine;
      }
      this.toasterService.info('Updated quantity for existing order line item.');
    } else {
      this.newOrder.lines.push({
        productId: this.selectedProductForLine,
        orderedQuantity: this.quantityForLine,
        unitPrice: this.priceForLine || 0
      });
    }

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
    return this.newOrder.lines.reduce((sum, l) => sum + (l.orderedQuantity || 0), 0);
  }

  getOrderTotalAmount(): number {
    if (!this.newOrder.lines) return 0;
    return this.newOrder.lines.reduce((sum, l) => sum + ((l.orderedQuantity || 0) * (l.unitPrice || 0)), 0);
  }

  saveOrder(): void {
    if (!this.newOrder.orderNumber || !this.newOrder.orderNumber.trim()) {
      this.toasterService.warn('Please enter a valid Sales Order Number.');
      return;
    }
    if (!this.newOrder.warehouseId) {
      this.toasterService.warn('Please select a fulfillment facility/warehouse.');
      return;
    }
    if (!this.newOrder.customerName || !this.newOrder.customerName.trim()) {
      this.toasterService.warn('Please select a registered customer account.');
      return;
    }
    if (!this.newOrder.lines || this.newOrder.lines.length === 0) {
      this.toasterService.warn('Please add at least one product item to the sales order before saving.');
      return;
    }

    this.newOrder.requiredDeliveryDate = this.deliveryDateStr 
      ? (new Date(this.deliveryDateStr).toISOString() as any) 
      : (new Date().toISOString() as any);

    this.isSaving = true;
    this.outboundService.create(this.newOrder).subscribe({
      next: (created) => {
        this.isSaving = false;
        this.isCreateOrderModalOpen = false;
        this.toasterService.success(`Sales Order ${created.orderNumber} created successfully!`, 'Order Created');
        this.loadOrders();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isSaving = false;
        this.toasterService.error(err?.error?.message || 'Failed to create Sales Order');
        this.cdr.detectChanges();
      }
    });
  }
}
