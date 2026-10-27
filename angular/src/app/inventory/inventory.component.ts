import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { InventoryService } from '../proxy/inventory';
import { 
  InventoryItemDto, 
  AdjustStockDto, 
  TransferStockDto, 
  InventoryTransactionDto 
} from '../proxy/inventory/models';
import { InventoryStatus } from '../proxy/inventory/inventory-status.enum';
import { WarehouseService, WarehouseLocationService } from '../proxy/warehouses';
import { WarehouseDto, WarehouseLocationDto } from '../proxy/warehouses/models';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventory.component.html',
  styleUrls: ['./inventory.component.scss']
})
export class InventoryComponent implements OnInit {
  private inventoryService = inject(InventoryService);
  private warehouseService = inject(WarehouseService);
  private locationService = inject(WarehouseLocationService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  inventoryItems: InventoryItemDto[] = [];
  filteredItems: InventoryItemDto[] = [];
  transactions: InventoryTransactionDto[] = [];
  warehouses: WarehouseDto[] = [];
  locations: WarehouseLocationDto[] = [];

  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedWarehouseFilter: string = '';
  activeTab: 'stock' | 'transactions' = 'stock';

  // Modals
  isAdjustModalOpen = false;
  isTransferModalOpen = false;
  selectedItemForAction: InventoryItemDto | null = null;

  adjustModel: AdjustStockDto = {
    warehouseId: '',
    locationId: '',
    productId: '',
    quantityDelta: 0,
    reason: 'Physical Audit Discrepancy',
    notes: ''
  };

  transferModel: TransferStockDto = {
    sourceInventoryItemId: '',
    destinationWarehouseId: '',
    destinationLocationId: '',
    quantity: 1,
    notes: 'Transfer to pick rack'
  };

  get totalOnHand(): number {
    return this.filteredItems.reduce((acc, i) => acc + (i.quantityOnHand || 0), 0);
  }

  get totalAllocated(): number {
    return this.filteredItems.reduce((acc, i) => acc + (i.quantityAllocated || 0), 0);
  }

  get totalAvailable(): number {
    return this.filteredItems.reduce((acc, i) => acc + (i.quantityAvailable || 0), 0);
  }

  get uniqueSkusCount(): number {
    const skus = new Set(this.filteredItems.map(i => i.productId));
    return skus.size;
  }

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadInventory();
  }

  loadWarehouses(): void {
    this.warehouseService.getList({ maxResultCount: 50, skipCount: 0 }).subscribe({
      next: (res) => this.warehouses = res.items || []
    });
  }

  loadInventory(): void {
    this.isLoading = true;
    this.inventoryService.getList({
      warehouseId: this.selectedWarehouseFilter || undefined,
      maxResultCount: 100,
      skipCount: 0
    }).subscribe({
      next: (res) => {
        this.inventoryItems = res.items || [];
        this.applyFilter();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadTransactions(): void {
    this.inventoryService.getTransactions({
      maxResultCount: 50,
      skipCount: 0
    }).subscribe({
      next: (res) => {
        this.transactions = res.items || [];
        this.cdr.detectChanges();
      }
    });
  }

  switchTab(tab: 'stock' | 'transactions'): void {
    this.activeTab = tab;
    if (tab === 'transactions' && this.transactions.length === 0) {
      this.loadTransactions();
    }
  }

  applyFilter(): void {
    this.filteredItems = this.inventoryItems.filter(item => {
      const q = this.searchQuery?.trim().toLowerCase() || '';
      const matchesSearch = !q ||
        item.productSku?.toLowerCase().includes(q) ||
        item.productName?.toLowerCase().includes(q) ||
        item.locationBarcode?.toLowerCase().includes(q) ||
        item.lotBatchNumber?.toLowerCase().includes(q);

      return matchesSearch;
    });
  }

  getStatusBadge(status: InventoryStatus | undefined): string {
    switch (status) {
      case InventoryStatus.Available: return 'wms-badge-success';
      case InventoryStatus.Quarantined: return 'wms-badge-warning';
      case InventoryStatus.Damaged: return 'wms-badge-danger';
      case InventoryStatus.Expired: return 'wms-badge-danger';
      default: return 'wms-badge-primary';
    }
  }

  getStatusLabel(status: InventoryStatus | undefined): string {
    switch (status) {
      case InventoryStatus.Available: return 'Available';
      case InventoryStatus.Quarantined: return 'Quarantined';
      case InventoryStatus.Damaged: return 'Damaged';
      case InventoryStatus.Expired: return 'Expired';
      default: return 'Standard';
    }
  }

  isStockDeduction(type: number | undefined): boolean {
    return type === 2 || type === 5 || type === 6;
  }

  getTransactionBadge(type: number | undefined): string {
    switch (type) {
      case 1: return 'wms-badge-success';
      case 2: return 'wms-badge-warning';
      case 3: return 'wms-badge-info';
      case 4: return 'wms-badge-success';
      case 5: return 'wms-badge-danger';
      case 6: return 'wms-badge-danger';
      default: return 'wms-badge-primary';
    }
  }

  getTransactionLabel(type: number | undefined): string {
    switch (type) {
      case 1: return 'Inbound Receipt';
      case 2: return 'Outbound Pick';
      case 3: return 'Bin Transfer';
      case 4: return 'Stock Increase';
      case 5: return 'Stock Decrease';
      case 6: return 'Damage Write-Off';
      case 7: return 'Quarantine Release';
      default: return 'Transaction';
    }
  }

  openAdjustModal(item: InventoryItemDto): void {
    this.selectedItemForAction = item;
    this.adjustModel = {
      warehouseId: item.warehouseId!,
      locationId: item.locationId!,
      productId: item.productId!,
      lotBatchNumber: item.lotBatchNumber,
      serialNumber: item.serialNumber,
      lpn: item.lpn,
      quantityDelta: 0,
      reason: 'Physical Audit Discrepancy',
      notes: ''
    };
    this.isSaving = false;
    this.isAdjustModalOpen = true;
    this.cdr.detectChanges();
  }

  closeAdjustModal(): void {
    this.isAdjustModalOpen = false;
    this.isSaving = false;
    this.selectedItemForAction = null;
    this.cdr.detectChanges();
  }

  saveAdjust(): void {
    if (this.adjustModel.quantityDelta === 0) {
      this.toasterService.warn('Please specify a positive or negative quantity delta to adjust.');
      return;
    }

    if (!this.adjustModel.reason?.trim()) {
      this.toasterService.warn('Please specify an audit reason for the stock adjustment.');
      return;
    }

    const currentQty = this.selectedItemForAction?.quantityOnHand || 0;
    if (currentQty + this.adjustModel.quantityDelta < 0) {
      this.toasterService.warn(`Cannot reduce stock below zero. Current stock is ${currentQty}.`);
      return;
    }

    this.isSaving = true;
    this.inventoryService.adjustStock(this.adjustModel).subscribe({
      next: () => {
        this.isSaving = false;
        this.isAdjustModalOpen = false;
        this.selectedItemForAction = null;
        this.toasterService.success('Stock balance adjusted successfully!', 'Audit Updated');
        this.loadInventory();
      },
      error: (err) => {
        this.isSaving = false;
        this.cdr.detectChanges();
        const serverMsg = err?.error?.error?.message || err?.message || 'Failed to adjust stock.';
        this.toasterService.error(serverMsg, 'Adjustment Error');
      }
    });
  }

  openTransferModal(item: InventoryItemDto): void {
    this.selectedItemForAction = item;
    this.transferModel = {
      sourceInventoryItemId: item.id!,
      destinationWarehouseId: item.warehouseId!,
      destinationLocationId: '',
      quantity: Math.min(10, item.quantityAvailable || 1),
      notes: 'Transfer to pick rack'
    };
    this.isSaving = false;

    // Load available locations for target warehouse
    this.locationService.getList({ warehouseId: item.warehouseId!, maxResultCount: 50, skipCount: 0 }).subscribe({
      next: (res) => {
        this.locations = (res.items || []).filter(l => l.id !== item.locationId);
        if (this.locations.length > 0) {
          this.transferModel.destinationLocationId = this.locations[0].id!;
        }
        this.isTransferModalOpen = true;
        this.cdr.detectChanges();
      }
    });
  }

  closeTransferModal(): void {
    this.isTransferModalOpen = false;
    this.isSaving = false;
    this.selectedItemForAction = null;
    this.cdr.detectChanges();
  }

  saveTransfer(): void {
    if (!this.transferModel.destinationLocationId) {
      this.toasterService.warn('Please select a target destination bin.');
      return;
    }

    if (this.transferModel.quantity <= 0) {
      this.toasterService.warn('Transfer quantity must be greater than zero.');
      return;
    }

    const available = this.selectedItemForAction?.quantityAvailable || 0;
    if (this.transferModel.quantity > available) {
      this.toasterService.warn(`Cannot transfer ${this.transferModel.quantity}. Only ${available} units are currently available.`);
      return;
    }

    this.isSaving = true;
    this.inventoryService.transferStock(this.transferModel).subscribe({
      next: () => {
        this.isSaving = false;
        this.isTransferModalOpen = false;
        this.selectedItemForAction = null;
        this.toasterService.success('Stock relocated to target bin successfully!', 'Transfer Complete');
        this.loadInventory();
      },
      error: (err) => {
        this.isSaving = false;
        this.cdr.detectChanges();
        const serverMsg = err?.error?.error?.message || err?.message || 'Failed to transfer stock.';
        this.toasterService.error(serverMsg, 'Transfer Error');
      }
    });
  }
}
