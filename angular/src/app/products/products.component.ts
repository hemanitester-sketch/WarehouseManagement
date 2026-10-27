import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { ProductService } from '../proxy/products';
import { ProductDto, CreateUpdateProductDto } from '../proxy/products/models';
import { ProductCategory, productCategoryOptions } from '../proxy/products/product-category.enum';
import { UnitOfMeasure, unitOfMeasureOptions } from '../proxy/products/unit-of-measure.enum';
import { WarehouseZoneStorageType, warehouseZoneStorageTypeOptions } from '../proxy/warehouses/warehouse-zone-storage-type.enum';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  private productService = inject(ProductService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  products: ProductDto[] = [];
  filteredProducts: ProductDto[] = [];
  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedCategoryFilter: number | null = null;

  isCreateModalOpen = false;
  editingProductId: string | null = null;

  newProduct: CreateUpdateProductDto = {
    sku: '',
    name: '',
    barcode: '',
    brand: '',
    description: '',
    category: ProductCategory.Pharmaceuticals,
    uom: UnitOfMeasure.Pieces,
    isLotTracked: true,
    isExpiryTracked: true,
    isSerialTracked: false,
    unitPrice: 150,
    reorderLevel: 50,
    targetStockLevel: 500,
    requiredStorageType: WarehouseZoneStorageType.ColdStorage,
    temperatureMinCelsius: 2,
    temperatureMaxCelsius: 8,
    weightKg: 0.5,
    volumeCm3: 100,
    isActive: true
  };

  categoryOptions = productCategoryOptions;
  uomOptions = unitOfMeasureOptions;
  storageTypeOptions = warehouseZoneStorageTypeOptions;

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getList({
      filter: '',
      skipCount: 0,
      maxResultCount: 100
    }).subscribe({
      next: (res) => {
        this.products = res.items || [];
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

  applyFilter(): void {
    this.filteredProducts = this.products.filter(p => {
      const q = this.searchQuery?.trim().toLowerCase() || '';
      const matchesSearch = !q ||
        p.sku?.toLowerCase().includes(q) ||
        p.name?.toLowerCase().includes(q) ||
        p.barcode?.toLowerCase().includes(q) ||
        p.brand?.toLowerCase().includes(q);

      const matchesCat = this.selectedCategoryFilter === null || p.category === this.selectedCategoryFilter;

      return matchesSearch && matchesCat;
    });
  }

  getCategoryLabel(cat: ProductCategory | undefined): string {
    const found = this.categoryOptions.find(c => c.value === cat);
    return found ? found.key : 'General';
  }

  getUomLabel(uom: UnitOfMeasure | undefined): string {
    const found = this.uomOptions.find(u => u.value === uom);
    return found ? found.key : 'Pieces';
  }

  getStorageTypeLabel(type: WarehouseZoneStorageType | undefined): string {
    const found = this.storageTypeOptions.find(s => s.value === type);
    return found ? found.key : 'Ambient';
  }

  openCreateModal(): void {
    this.editingProductId = null;
    this.newProduct = {
      sku: '',
      name: '',
      barcode: '',
      brand: '',
      description: '',
      category: ProductCategory.Pharmaceuticals,
      uom: UnitOfMeasure.Pieces,
      isLotTracked: true,
      isExpiryTracked: true,
      isSerialTracked: false,
      unitPrice: 150,
      reorderLevel: 50,
      targetStockLevel: 500,
      requiredStorageType: WarehouseZoneStorageType.ColdStorage,
      temperatureMinCelsius: 2,
      temperatureMaxCelsius: 8,
      weightKg: 0.5,
      volumeCm3: 100,
      isActive: true
    };
    this.isSaving = false;
    this.isCreateModalOpen = true;
    this.cdr.detectChanges();
  }

  openEditModal(p: ProductDto): void {
    this.editingProductId = p.id || null;
    this.newProduct = {
      sku: p.sku || '',
      name: p.name || '',
      barcode: p.barcode || '',
      brand: p.brand || '',
      description: p.description || '',
      category: p.category ?? ProductCategory.Pharmaceuticals,
      uom: p.uom ?? UnitOfMeasure.Pieces,
      isLotTracked: p.isLotTracked ?? false,
      isExpiryTracked: p.isExpiryTracked ?? false,
      isSerialTracked: p.isSerialTracked ?? false,
      unitPrice: p.unitPrice ?? 0,
      reorderLevel: p.reorderLevel ?? 10,
      targetStockLevel: p.targetStockLevel ?? 100,
      requiredStorageType: p.requiredStorageType ?? WarehouseZoneStorageType.Ambient,
      temperatureMinCelsius: p.temperatureMinCelsius ?? null,
      temperatureMaxCelsius: p.temperatureMaxCelsius ?? null,
      weightKg: p.weightKg ?? 0.5,
      volumeCm3: p.volumeCm3 ?? 100,
      isActive: p.isActive ?? true
    };
    this.isSaving = false;
    this.isCreateModalOpen = true;
    this.cdr.detectChanges();
  }

  closeCreateModal(): void {
    this.isCreateModalOpen = false;
    this.editingProductId = null;
    this.isSaving = false;
    this.cdr.detectChanges();
  }

  saveProduct(): void {
    if (!this.newProduct.sku?.trim()) {
      this.toasterService.warn('Please enter a SKU Code for the product.');
      return;
    }

    if (!this.newProduct.name?.trim()) {
      this.toasterService.warn('Please enter a Product Name.');
      return;
    }

    // Client-side duplicate SKU check (excluding currently edited product)
    const normalizedSku = this.newProduct.sku.trim().toUpperCase();
    if (this.products.some(p => p.id !== this.editingProductId && p.sku?.toUpperCase() === normalizedSku)) {
      this.toasterService.warn(`SKU '${normalizedSku}' already exists in your product catalog. Please use a unique SKU code.`);
      return;
    }

    // Client-side duplicate Barcode check (excluding currently edited product)
    if (this.newProduct.barcode?.trim()) {
      const normalizedBarcode = this.newProduct.barcode.trim();
      if (this.products.some(p => p.id !== this.editingProductId && p.barcode === normalizedBarcode)) {
        this.toasterService.warn(`Barcode '${normalizedBarcode}' is already assigned to another product.`);
        return;
      }
    }

    // Temperature check
    if (this.newProduct.temperatureMinCelsius !== null && 
        this.newProduct.temperatureMinCelsius !== undefined &&
        this.newProduct.temperatureMaxCelsius !== null && 
        this.newProduct.temperatureMaxCelsius !== undefined) {
      if (this.newProduct.temperatureMinCelsius > this.newProduct.temperatureMaxCelsius) {
        this.toasterService.warn('Minimum temperature cannot be higher than maximum temperature.');
        return;
      }
    }

    this.isSaving = true;
    this.newProduct.sku = normalizedSku;

    if (this.editingProductId) {
      // Update existing product
      this.productService.update(this.editingProductId, this.newProduct).subscribe({
        next: () => {
          this.isSaving = false;
          this.isCreateModalOpen = false;
          this.editingProductId = null;
          this.toasterService.success('Product SKU updated successfully!', 'Updated');
          this.loadProducts();
        },
        error: (err) => {
          this.isSaving = false;
          this.cdr.detectChanges();
          const serverMsg = err?.error?.error?.message || err?.message || 'Failed to update product SKU.';
          this.toasterService.error(serverMsg, 'Validation Error');
        }
      });
    } else {
      // Create new product
      this.productService.create(this.newProduct).subscribe({
        next: () => {
          this.isSaving = false;
          this.isCreateModalOpen = false;
          this.editingProductId = null;
          this.toasterService.success('Product SKU registered successfully!', 'Success');
          this.loadProducts();
        },
        error: (err) => {
          this.isSaving = false;
          this.cdr.detectChanges();
          const serverMsg = err?.error?.error?.message || err?.message || 'Failed to register product SKU.';
          this.toasterService.error(serverMsg, 'Validation Error');
        }
      });
    }
  }
}
