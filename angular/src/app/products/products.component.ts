import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { ProductService, CategoryService, ProductVariantService, ProductAttributeService } from '../proxy/products';
import {
  ProductDto,
  CreateUpdateProductDto,
  CategoryLookupDto,
  ProductType,
  ProductVariantDto,
  CreateProductVariantDto,
  ProductAttributeDto,
  AttributeMatrixOptionInput,
  GeneratedVariantPreviewDto,
  BarcodeSymbology,
  BarcodePackagingLevel
} from '../proxy/products/models';
import { ProductCategory, productCategoryOptions } from '../proxy/products/product-category.enum';
import { UnitOfMeasure, unitOfMeasureOptions } from '../proxy/products/unit-of-measure.enum';
import { WarehouseZoneStorageType, warehouseZoneStorageTypeOptions } from '../proxy/warehouses/warehouse-zone-storage-type.enum';
import { SupplierService } from '../proxy/suppliers/supplier.service';
import { SupplierLookupDto } from '../proxy/suppliers/models';
import {
  BarcodeLookupService,
  BarcodeLookupResultDto,
  SupplierBarcodeService,
  SupplierBarcodeMappingDto,
  CreateSupplierBarcodeMappingDto
} from '../proxy/barcodes';
import { BarcodeVisualComponent } from '../shared/components/barcode-visual/barcode-visual.component';
import { BarcodeScannerDirective } from '../shared/directives/barcode-scanner.directive';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, BarcodeVisualComponent, BarcodeScannerDirective],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private variantService = inject(ProductVariantService);
  private attributeService = inject(ProductAttributeService);
  private supplierService = inject(SupplierService);
  private supplierBarcodeService = inject(SupplierBarcodeService);
  private barcodeLookupService = inject(BarcodeLookupService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  // Enum references for template
  ProductType = ProductType;
  BarcodeSymbology = BarcodeSymbology;
  BarcodePackagingLevel = BarcodePackagingLevel;

  products: ProductDto[] = [];
  filteredProducts: ProductDto[] = [];
  categoryLookups: CategoryLookupDto[] = [];
  supplierLookups: SupplierLookupDto[] = [];
  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedCategoryFilter: string | number | null = null;
  quickScanInput = '';

  // Multi-Select Checkbox State
  selectedProductIds: Set<string> = new Set<string>();

  isAllSelected(): boolean {
    const list = this.paginatedProducts;
    return list.length > 0 && list.every(p => p.id && this.selectedProductIds.has(p.id));
  }

  isPartiallySelected(): boolean {
    const list = this.paginatedProducts;
    const selectedCount = list.filter(p => p.id && this.selectedProductIds.has(p.id)).length;
    return selectedCount > 0 && selectedCount < list.length;
  }

  toggleSelectAll(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const list = this.paginatedProducts;
    if (checked) {
      list.forEach(p => {
        if (p.id) this.selectedProductIds.add(p.id);
      });
    } else {
      list.forEach(p => {
        if (p.id) this.selectedProductIds.delete(p.id);
      });
    }
  }

  toggleSelectProduct(productId: string | undefined): void {
    if (!productId) return;
    if (this.selectedProductIds.has(productId)) {
      this.selectedProductIds.delete(productId);
    } else {
      this.selectedProductIds.add(productId);
    }
  }

  isProductSelected(productId: string | undefined): boolean {
    return !!productId && this.selectedProductIds.has(productId);
  }

  // Pagination State
  currentPage = 1;
  pageSize = 50;

  get paginatedProducts(): ProductDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredProducts.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredProducts.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    return this.filteredProducts.length === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredProducts.length);
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

  // Drawer / Modal State
  isCreateModalOpen = false;
  editingProductId: string | null = null;
  activeDrawerTab: 'basic' | 'variants' | 'barcodes' | 'labels' = 'basic';

  newProduct: CreateUpdateProductDto = {
    sku: '',
    name: '',
    productType: ProductType.Simple,
    barcode: '',
    brand: '',
    description: '',
    category: ProductCategory.Pharmaceuticals,
    categoryId: null,
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
    imageUrl: '',
    isActive: true
  };

  // Real-time Uniqueness Validation State
  isSkuDuplicate = false;
  skuTouched = false;
  isCheckingSku = false;
  private skuCheckTimer: any = null;

  isBarcodeDuplicate = false;
  barcodeTouched = false;
  isCheckingBarcode = false;
  private barcodeCheckTimer: any = null;

  isProductNameDuplicate = false;
  nameTouched = false;
  isCheckingName = false;
  private nameCheckTimer: any = null;

  isUploadingImage = false;

  resolveImageUrl(url?: string | null): string {
    if (!url) return '';
    if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) {
      return url;
    }
    const baseUrl = (environment.apis?.default?.url || 'https://localhost:44319').replace(/\/+$/, '');
    return `${baseUrl}/${url.replace(/^\/+/, '')}`;
  }

  onProductImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];

    // Immediate preview via FileReader
    const reader = new FileReader();
    reader.onload = () => {
      this.newProduct.imageUrl = reader.result as string;
      this.cdr.detectChanges();
    };
    reader.readAsDataURL(file);

    // Upload to server
    this.isUploadingImage = true;
    this.productService.uploadImage(file).subscribe({
      next: (res) => {
        if (res && res.url) {
          this.newProduct.imageUrl = res.url;
        }
        this.isUploadingImage = false;
        this.toasterService.success('Product image uploaded successfully.');
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isUploadingImage = false;
        console.warn('Image upload fallback to preview', err);
        this.cdr.detectChanges();
      }
    });
  }

  removeProductImage(): void {
    this.newProduct.imageUrl = '';
  }

  onVariantImageSelected(event: Event, pv: GeneratedVariantPreviewDto): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];

    const reader = new FileReader();
    reader.onload = () => {
      pv.imageUrl = reader.result as string;
      this.cdr.detectChanges();
    };
    reader.readAsDataURL(file);

    this.productService.uploadImage(file).subscribe({
      next: (res) => {
        if (res && res.url) {
          pv.imageUrl = res.url;
        }
        this.toasterService.success(`Image uploaded for variant ${pv.generatedSku}.`);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.warn('Variant image upload fallback to preview', err);
        this.cdr.detectChanges();
      }
    });
  }

  removeVariantImage(pv: GeneratedVariantPreviewDto): void {
    pv.imageUrl = '';
  }

  categoryOptions = productCategoryOptions;
  uomOptions = unitOfMeasureOptions;
  storageTypeOptions = warehouseZoneStorageTypeOptions;

  // Variants and Attributes State
  availableAttributes: ProductAttributeDto[] = [];
  selectedAttributeIds: string[] = [];
  attributeValuesMap: { [attrId: string]: string[] } = {};
  customAttrInputs: { [attrId: string]: string } = {};
  generatedVariantPreview: GeneratedVariantPreviewDto[] = [];
  isGeneratingMatrix = false;
  existingVariants: ProductVariantDto[] = [];
  isLoadingVariants = false;
  expandedProductId: string | null = null;
  expandedProductVariants: { [productId: string]: ProductVariantDto[] } = {};

  // Supplier Barcode Mappings State
  supplierMappings: SupplierBarcodeMappingDto[] = [];
  isLoadingSupplierMappings = false;
  isSavingSupplierMapping = false;
  newSupplierMapping: {
    supplierId: string;
    productVariantId: string;
    supplierBarcode: string;
    supplierProductCode: string;
    packagingLevel: number;
    unitsPerPackage: number;
  } = {
    supplierId: '',
    productVariantId: '',
    supplierBarcode: '',
    supplierProductCode: '',
    packagingLevel: BarcodePackagingLevel.Each,
    unitsPerPackage: 1
  };

  // Barcode Lookup Modal State
  isLookupModalOpen = false;
  isResolvingBarcode = false;
  lookupResult: BarcodeLookupResultDto | null = null;

  // Label Print Modal State
  isPrintModalOpen = false;
  printProduct: ProductDto | null = null;
  printVariant: ProductVariantDto | null = null;
  printBarcodeValue = '';
  printTitle = '';
  printSku = '';
  printFormat: '2x1' | '4x6' = '2x1';
  printQuantity = 1;

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategoryLookups();
    this.loadSupplierLookups();
    this.loadAttributes();
  }

  loadCategoryLookups(): void {
    this.categoryService.getLookup().subscribe({
      next: (res) => {
        this.categoryLookups = res.items || [];
        this.cdr.detectChanges();
      },
      error: (err) => console.warn('Could not load category lookups:', err),
    });
  }

  loadSupplierLookups(): void {
    this.supplierService.getLookup().subscribe({
      next: (res) => {
        this.supplierLookups = res.items || [];
        this.cdr.detectChanges();
      },
      error: (err) => console.warn('Could not load supplier lookups:', err),
    });
  }

  loadAttributes(): void {
    this.attributeService.getList().subscribe({
      next: (attrs) => {
        this.availableAttributes = attrs || [];
        this.cdr.detectChanges();
      },
      error: (err) => console.warn('Could not load attributes:', err),
    });
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

      let matchesCat = true;
      if (this.selectedCategoryFilter !== null && this.selectedCategoryFilter !== undefined) {
        if (typeof this.selectedCategoryFilter === 'string') {
          matchesCat = p.categoryId === this.selectedCategoryFilter;
        } else {
          matchesCat = p.category === this.selectedCategoryFilter;
        }
      }

      return matchesSearch && matchesCat;
    });
    this.currentPage = 1;
  }

  getCategoryLabel(cat: ProductCategory | undefined): string {
    const found = this.categoryOptions.find(c => c.value === cat);
    return found ? found.key : 'General';
  }

  formatCategoryOption(c: CategoryLookupDto | undefined): string {
    if (!c) return '';
    const text = c.displayName || c.name || '';
    if (text.length > 34) {
      return text.substring(0, 32) + '...';
    }
    return text;
  }

  getUomLabel(uom: UnitOfMeasure | undefined): string {
    const found = this.uomOptions.find(u => u.value === uom);
    return found ? found.key : 'Pieces';
  }

  getStorageTypeLabel(type: WarehouseZoneStorageType | undefined): string {
    const found = this.storageTypeOptions.find(s => s.value === type);
    return found ? found.key : 'Ambient';
  }

  getVariantCount(p: ProductDto): number {
    if (p.id && this.expandedProductVariants[p.id]?.length !== undefined) {
      return this.expandedProductVariants[p.id].length;
    }
    return p.variantCount ?? 0;
  }

  toggleExpandVariants(p: ProductDto): void {
    if (!p.id) return;
    if (this.expandedProductId === p.id) {
      this.expandedProductId = null;
      return;
    }

    this.expandedProductId = p.id;
    if (!this.expandedProductVariants[p.id]) {
      this.variantService.getListByProductId(p.id).subscribe({
        next: (variants) => {
          this.expandedProductVariants[p.id!] = variants || [];
          p.variantCount = variants ? variants.length : 0;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.toasterService.error('Failed to load variants.');
          console.warn(err);
        }
      });
    }
  }

  private resetValidationState(): void {
    if (this.skuCheckTimer) clearTimeout(this.skuCheckTimer);
    if (this.barcodeCheckTimer) clearTimeout(this.barcodeCheckTimer);
    if (this.nameCheckTimer) clearTimeout(this.nameCheckTimer);

    this.isSkuDuplicate = false;
    this.skuTouched = false;
    this.isCheckingSku = false;

    this.isBarcodeDuplicate = false;
    this.barcodeTouched = false;
    this.isCheckingBarcode = false;

    this.isProductNameDuplicate = false;
    this.nameTouched = false;
    this.isCheckingName = false;
  }

  // --- Real-time SKU Validation ---
  onSkuInput(): void {
    this.skuTouched = true;
    const sku = (this.newProduct.sku || '').trim().toUpperCase();
    if (this.skuCheckTimer) clearTimeout(this.skuCheckTimer);

    if (!sku) {
      this.isCheckingSku = false;
      this.isSkuDuplicate = false;
      return;
    }

    this.isCheckingSku = true;
    this.skuCheckTimer = setTimeout(() => {
      this.checkSkuUniqueness();
      this.isCheckingSku = false;
      this.cdr.detectChanges();
    }, 300);
  }

  onSkuBlur(): void {
    if (this.skuCheckTimer) clearTimeout(this.skuCheckTimer);
    this.checkSkuUniqueness();
    this.isCheckingSku = false;
  }

  checkSkuUniqueness(): void {
    this.skuTouched = true;
    const sku = (this.newProduct.sku || '').trim().toUpperCase();
    if (!sku) {
      this.isSkuDuplicate = false;
      return;
    }

    const match = this.products.find(
      (p) => p.sku?.trim().toUpperCase() === sku && p.id !== this.editingProductId
    );

    this.isSkuDuplicate = !!match;
  }

  // --- Real-time Barcode Validation ---
  onBarcodeValidationInput(): void {
    this.barcodeTouched = true;
    const barcode = (this.newProduct.barcode || '').trim();
    if (this.barcodeCheckTimer) clearTimeout(this.barcodeCheckTimer);

    if (!barcode) {
      this.isCheckingBarcode = false;
      this.isBarcodeDuplicate = false;
      return;
    }

    this.isCheckingBarcode = true;
    this.barcodeCheckTimer = setTimeout(() => {
      this.checkBarcodeUniqueness();
      this.isCheckingBarcode = false;
      this.cdr.detectChanges();
    }, 300);
  }

  onBarcodeValidationBlur(): void {
    if (this.barcodeCheckTimer) clearTimeout(this.barcodeCheckTimer);
    this.checkBarcodeUniqueness();
    this.isCheckingBarcode = false;
  }

  checkBarcodeUniqueness(): void {
    this.barcodeTouched = true;
    const barcode = (this.newProduct.barcode || '').trim();
    if (!barcode) {
      this.isBarcodeDuplicate = false;
      return;
    }

    const match = this.products.find(
      (p) => p.barcode?.trim() === barcode && p.id !== this.editingProductId
    );

    this.isBarcodeDuplicate = !!match;
  }

  // --- Real-time Product Name Validation (under same Brand & Category) ---
  onProductNameInput(): void {
    this.nameTouched = true;
    const name = (this.newProduct.name || '').trim();
    if (this.nameCheckTimer) clearTimeout(this.nameCheckTimer);

    if (!name) {
      this.isCheckingName = false;
      this.isProductNameDuplicate = false;
      return;
    }

    this.isCheckingName = true;
    this.nameCheckTimer = setTimeout(() => {
      this.checkProductNameUniqueness();
      this.isCheckingName = false;
      this.cdr.detectChanges();
    }, 300);
  }

  onProductNameBlur(): void {
    if (this.nameCheckTimer) clearTimeout(this.nameCheckTimer);
    this.checkProductNameUniqueness();
    this.isCheckingName = false;
  }

  onBrandInput(): void {
    if (this.newProduct.name?.trim()) {
      this.checkProductNameUniqueness();
    }
  }

  onCategoryChange(): void {
    if (this.newProduct.name?.trim()) {
      this.checkProductNameUniqueness();
    }
  }

  checkProductNameUniqueness(): void {
    this.nameTouched = true;
    const name = (this.newProduct.name || '').trim().toLowerCase();
    if (!name) {
      this.isProductNameDuplicate = false;
      return;
    }

    const brand = (this.newProduct.brand || '').trim().toLowerCase();
    const catId = this.newProduct.categoryId;

    const match = this.products.find((p) => {
      if (p.id === this.editingProductId) return false;
      const sameName = (p.name || '').trim().toLowerCase() === name;
      if (!sameName) return false;

      const sameBrand = (p.brand || '').trim().toLowerCase() === brand;
      if (!sameBrand) return false;

      const pCatId = p.categoryId;
      if (catId && pCatId) {
        return catId === pCatId;
      } else if (!catId && !pCatId) {
        return p.category === this.newProduct.category;
      }
      return false;
    });

    this.isProductNameDuplicate = !!match;
  }

  openCreateModal(): void {
    this.resetValidationState();
    this.editingProductId = null;
    this.activeDrawerTab = 'basic';
    this.newProduct = {
      sku: '',
      name: '',
      productType: ProductType.Simple,
      barcode: '',
      brand: '',
      description: '',
      category: ProductCategory.Pharmaceuticals,
      categoryId: null,
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
      imageUrl: '',
      isActive: true
    };
    this.selectedAttributeIds = [];
    this.attributeValuesMap = {};
    this.generatedVariantPreview = [];
    this.existingVariants = [];
    this.supplierMappings = [];
    this.isSaving = false;
    this.isCreateModalOpen = true;
    this.cdr.detectChanges();
  }

  openEditModal(p: ProductDto): void {
    this.resetValidationState();
    this.editingProductId = p.id || null;
    this.activeDrawerTab = 'basic';
    this.newProduct = {
      sku: p.sku || '',
      name: p.name || '',
      productType: p.productType ?? ProductType.Simple,
      barcode: p.barcode || '',
      brand: p.brand || '',
      description: p.description || '',
      category: p.category ?? ProductCategory.Pharmaceuticals,
      categoryId: p.categoryId ?? null,
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
      imageUrl: p.imageUrl || '',
      isActive: p.isActive ?? true
    };

    if (this.newProduct.sku) this.checkSkuUniqueness();
    if (this.newProduct.barcode) this.checkBarcodeUniqueness();
    if (this.newProduct.name) this.checkProductNameUniqueness();

    this.selectedAttributeIds = [];
    this.attributeValuesMap = {};
    this.generatedVariantPreview = [];
    this.existingVariants = [];
    this.supplierMappings = [];
    this.isSaving = false;
    this.isCreateModalOpen = true;

    if (p.id) {
      this.loadProductVariants(p.id);
      this.loadProductSupplierMappings(p.id);
    }
    this.cdr.detectChanges();
  }

  closeCreateModal(): void {
    this.resetValidationState();
    this.isCreateModalOpen = false;
    this.editingProductId = null;
    this.isSaving = false;
    this.cdr.detectChanges();
  }

  switchDrawerTab(tab: 'basic' | 'variants' | 'barcodes' | 'labels'): void {
    this.activeDrawerTab = tab;
    this.cdr.detectChanges();
  }

  loadProductVariants(productId: string): void {
    this.isLoadingVariants = true;
    this.variantService.getListByProductId(productId).subscribe({
      next: (variants) => {
        this.existingVariants = variants || [];
        this.isLoadingVariants = false;
        if (this.existingVariants.length > 0 && !this.newSupplierMapping.productVariantId) {
          this.newSupplierMapping.productVariantId = this.existingVariants[0].id!;
        }
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoadingVariants = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadProductSupplierMappings(productId: string): void {
    this.isLoadingSupplierMappings = true;
    this.supplierBarcodeService.getList({
      productId: productId,
      skipCount: 0,
      maxResultCount: 100
    }).subscribe({
      next: (res) => {
        this.supplierMappings = res.items || [];
        this.isLoadingSupplierMappings = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoadingSupplierMappings = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Custom Attribute Creation
  isAddingCustomAttribute = false;
  newCustomAttribute = {
    name: '',
    code: '',
    valuesText: ''
  };

  openAddCustomAttribute(): void {
    this.isAddingCustomAttribute = true;
    this.newCustomAttribute = { name: '', code: '', valuesText: '' };
    this.cdr.detectChanges();
  }

  cancelAddCustomAttribute(): void {
    this.isAddingCustomAttribute = false;
    this.cdr.detectChanges();
  }

  saveCustomAttribute(): void {
    const name = this.newCustomAttribute.name.trim();
    if (!name) {
      this.toasterService.warn('Please enter an attribute name (e.g. Fabric, Material, Pattern).');
      return;
    }
    const code = (this.newCustomAttribute.code.trim() || name).toUpperCase().replace(/[^A-Z0-9_]/g, '_');
    const rawValues = this.newCustomAttribute.valuesText.split(',').map(s => s.trim()).filter(s => !!s);

    const valuesPayload = rawValues.map((v, i) => ({
      value: v,
      displayLabel: v,
      displayOrder: i + 1
    }));

    this.attributeService.create({
      name: name,
      code: code,
      dataType: 1, // Dropdown
      isVariantDefining: true,
      isRequired: false,
      displayOrder: this.availableAttributes.length + 1,
      isActive: true,
      values: valuesPayload
    }).subscribe({
      next: (created) => {
        this.toasterService.success(`Attribute '${created.name}' created successfully!`);
        this.isAddingCustomAttribute = false;
        this.availableAttributes.push(created);
        if (!this.selectedAttributeIds.includes(created.id!)) {
          this.selectedAttributeIds.push(created.id!);
          this.attributeValuesMap[created.id!] = (created.values || []).map(v => v.value);
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        const msg = err?.error?.error?.message || err?.message || 'Failed to create attribute.';
        this.toasterService.error(msg);
      }
    });
  }

  // --- Dynamic Attributes & Variant Matrix Generator ---
  toggleAttributeSelection(attrId: string): void {
    const idx = this.selectedAttributeIds.indexOf(attrId);
    if (idx > -1) {
      this.selectedAttributeIds.splice(idx, 1);
      delete this.attributeValuesMap[attrId];
    } else {
      this.selectedAttributeIds.push(attrId);
      const attr = this.availableAttributes.find(a => a.id === attrId);
      this.attributeValuesMap[attrId] = (attr?.values || []).map(v => v.value);
    }
    this.cdr.detectChanges();
  }

  deselectAttribute(attrId: string): void {
    const idx = this.selectedAttributeIds.indexOf(attrId);
    if (idx > -1) {
      this.selectedAttributeIds.splice(idx, 1);
      delete this.attributeValuesMap[attrId];
    }
    this.cdr.detectChanges();
  }

  isAttributeSelected(attrId: string): boolean {
    return this.selectedAttributeIds.includes(attrId);
  }

  getAttributeName(attrId: string): string {
    const found = this.availableAttributes.find(a => a.id === attrId);
    return found ? `${found.name} (${found.code})` : 'Attribute';
  }

  addCustomAttributeValue(attrId: string): void {
    const val = this.customAttrInputs[attrId]?.trim();
    if (!val) return;
    if (!this.attributeValuesMap[attrId]) {
      this.attributeValuesMap[attrId] = [];
    }
    if (!this.attributeValuesMap[attrId].includes(val)) {
      this.attributeValuesMap[attrId].push(val);
    }
    this.customAttrInputs[attrId] = '';
    this.cdr.detectChanges();
  }

  removeCustomAttributeValue(attrId: string, val: string): void {
    if (!this.attributeValuesMap[attrId]) return;
    this.attributeValuesMap[attrId] = this.attributeValuesMap[attrId].filter(v => v !== val);
    this.cdr.detectChanges();
  }

  generateVariantMatrix(): void {
    if (!this.editingProductId) {
      this.toasterService.warn('Please save the product first before generating variants.');
      return;
    }
    if (this.selectedAttributeIds.length === 0) {
      this.toasterService.warn('Please select at least one attribute to define variants.');
      return;
    }

    const options: AttributeMatrixOptionInput[] = [];
    for (const attrId of this.selectedAttributeIds) {
      const attr = this.availableAttributes.find(a => a.id === attrId);
      const vals = (this.attributeValuesMap[attrId] || []).filter(v => !!v?.trim());
      if (vals.length > 0) {
        options.push({
          attributeId: attrId,
          attributeName: attr?.name || '',
          attributeCode: attr?.code || '',
          selectedValues: vals
        });
      }
    }

    if (options.length === 0) {
      this.toasterService.warn('Please select at least one attribute with values (e.g. Size, Color, or Fabric).');
      return;
    }

    this.isGeneratingMatrix = true;
    this.variantService.generateVariantMatrixPreview({
      productId: this.editingProductId,
      options: options
    }).subscribe({
      next: (preview) => {
        this.generatedVariantPreview = preview || [];
        this.isGeneratingMatrix = false;
        this.toasterService.info(`Generated ${this.generatedVariantPreview.length} variant combinations.`);
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isGeneratingMatrix = false;
        this.toasterService.error('Failed to preview variant matrix.');
        console.error(err);
      }
    });
  }

  saveGeneratedVariants(): void {
    if (!this.editingProductId || this.generatedVariantPreview.length === 0) return;

    const variantsToCreate: CreateProductVariantDto[] = this.generatedVariantPreview.map(p => ({
      productId: this.editingProductId!,
      sku: p.generatedSku,
      variantName: p.generatedVariantName,
      purchasePrice: 0,
      sellingPrice: this.newProduct.unitPrice ?? 0,
      weightKg: this.newProduct.weightKg ?? 0.5,
      volumeCm3: this.newProduct.volumeCm3 ?? 100,
      imageUrl: p.imageUrl || this.newProduct.imageUrl || null,
      isDefault: false,
      attributes: p.attributes
    }));

    this.isSaving = true;
    this.variantService.batchCreateVariants(this.editingProductId, variantsToCreate).subscribe({
      next: (created) => {
        this.isSaving = false;
        this.generatedVariantPreview = [];
        this.toasterService.success(`Successfully registered ${created.length} variants!`, 'Variants Created');
        this.loadProductVariants(this.editingProductId!);
        this.loadProducts();
      },
      error: (err) => {
        this.isSaving = false;
        this.cdr.detectChanges();
        const serverMsg = err?.error?.error?.message || err?.message || 'Failed to create product variants.';
        this.toasterService.error(serverMsg, 'Validation Error');
      }
    });
  }

  // --- Supplier Barcode Mappings ---
  addSupplierBarcodeMapping(): void {
    if (!this.newSupplierMapping.supplierId) {
      this.toasterService.warn('Please select a Supplier.');
      return;
    }
    if (!this.newSupplierMapping.productVariantId) {
      this.toasterService.warn('Please select a Product Variant.');
      return;
    }
    if (!this.newSupplierMapping.supplierBarcode?.trim()) {
      this.toasterService.warn('Please enter the Supplier Barcode value.');
      return;
    }

    const payload: CreateSupplierBarcodeMappingDto = {
      supplierId: this.newSupplierMapping.supplierId,
      productVariantId: this.newSupplierMapping.productVariantId,
      supplierBarcode: this.newSupplierMapping.supplierBarcode.trim(),
      supplierProductCode: this.newSupplierMapping.supplierProductCode?.trim() || null,
      packagingLevel: Number(this.newSupplierMapping.packagingLevel),
      unitsPerPackage: Number(this.newSupplierMapping.unitsPerPackage) || 1
    };

    this.isSavingSupplierMapping = true;
    this.supplierBarcodeService.create(payload).subscribe({
      next: () => {
        this.isSavingSupplierMapping = false;
        this.newSupplierMapping.supplierBarcode = '';
        this.newSupplierMapping.supplierProductCode = '';
        this.toasterService.success('Supplier barcode mapped successfully!', 'Mapped');
        if (this.editingProductId) {
          this.loadProductSupplierMappings(this.editingProductId);
        }
      },
      error: (err) => {
        this.isSavingSupplierMapping = false;
        this.cdr.detectChanges();
        const serverMsg = err?.error?.error?.message || err?.message || 'Failed to map supplier barcode (Closed-Triangle rule check failed).';
        this.toasterService.error(serverMsg, 'Barcode Mapping Error');
      }
    });
  }

  deleteSupplierMapping(id: string): void {
    if (!confirm('Are you sure you want to remove this supplier barcode mapping?')) return;
    this.supplierBarcodeService.delete(id).subscribe({
      next: () => {
        this.toasterService.info('Supplier barcode mapping removed.');
        if (this.editingProductId) {
          this.loadProductSupplierMappings(this.editingProductId);
        }
      },
      error: () => this.toasterService.error('Failed to remove barcode mapping.')
    });
  }

  // --- Fast Barcode Scanner & Quick Lookup ---
  onBarcodeScanned(scannedCode: string): void {
    if (!scannedCode?.trim()) return;
    this.quickScanInput = scannedCode.trim();
    this.triggerQuickLookup(this.quickScanInput);
  }

  triggerQuickLookup(code?: string): void {
    const barcode = (code || this.quickScanInput)?.trim();
    if (!barcode) {
      this.toasterService.warn('Please enter or scan a barcode.');
      return;
    }

    this.isLookupModalOpen = true;
    this.isResolvingBarcode = true;
    this.lookupResult = null;
    this.cdr.detectChanges();

    this.barcodeLookupService.resolveBarcode({ barcodeValue: barcode }).subscribe({
      next: (result) => {
        this.isResolvingBarcode = false;
        this.lookupResult = result;
        if (!result.isFound) {
          this.toasterService.info(`No active warehouse entity found for barcode '${barcode}'.`);
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isResolvingBarcode = false;
        this.cdr.detectChanges();
        this.toasterService.error('Barcode lookup query failed.');
        console.error(err);
      }
    });
  }

  closeLookupModal(): void {
    this.isLookupModalOpen = false;
    this.lookupResult = null;
    this.quickScanInput = '';
    this.cdr.detectChanges();
  }

  getBarcodeTargetTypeName(type?: number | null): string {
    switch (type) {
      case 1: return 'Product Variant / SKU';
      case 2: return 'License Plate (LPN)';
      case 3: return 'Warehouse Location';
      case 4: return 'Inventory Lot / Batch';
      case 5: return 'Inventory Serial Number';
      default: return 'Unknown Target';
    }
  }

  // --- Barcode Label Print Center ---
  openPrintLabelModal(p: ProductDto, v?: ProductVariantDto): void {
    this.printProduct = p;
    this.printVariant = v || null;
    this.printTitle = v?.variantName || p.name || '';
    this.printSku = v?.sku || p.sku || '';
    this.printBarcodeValue = v?.primaryBarcode || p.barcode || p.sku || '';
    this.printFormat = '2x1';
    this.printQuantity = 1;
    this.isPrintModalOpen = true;
    this.cdr.detectChanges();
  }

  closePrintModal(): void {
    this.isPrintModalOpen = false;
    this.printProduct = null;
    this.printVariant = null;
    this.cdr.detectChanges();
  }

  executePrint(): void {
    window.print();
  }

  // --- Save / Create Product ---
  saveProduct(): void {
    if (!this.newProduct.sku?.trim()) {
      this.toasterService.warn('Please enter a SKU Code for the product.');
      return;
    }

    if (!this.newProduct.name?.trim()) {
      this.toasterService.warn('Please enter a Product Name.');
      return;
    }

    this.checkSkuUniqueness();
    if (this.isSkuDuplicate) {
      this.toasterService.error(`SKU '${this.newProduct.sku}' already exists in your product catalog. Please enter a unique SKU.`, 'Duplicate SKU');
      return;
    }

    if (this.newProduct.barcode?.trim()) {
      this.checkBarcodeUniqueness();
      if (this.isBarcodeDuplicate) {
        this.toasterService.error(`Barcode '${this.newProduct.barcode}' is already assigned to another product. Please enter a unique barcode.`, 'Duplicate Barcode');
        return;
      }
    }

    this.checkProductNameUniqueness();
    if (this.isProductNameDuplicate) {
      this.toasterService.error(`A product with name '${this.newProduct.name}' already exists under the same Brand and Category.`, 'Duplicate Product');
      return;
    }

    const normalizedSku = this.newProduct.sku.trim().toUpperCase();

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
      this.productService.update(this.editingProductId, this.newProduct).subscribe({
        next: (updated) => {
          this.isSaving = false;
          this.toasterService.success('Product updated successfully!', 'Updated');
          this.loadProducts();
          if (this.newProduct.productType === ProductType.VariantBased) {
            this.activeDrawerTab = 'variants';
            this.loadProductVariants(this.editingProductId!);
          } else {
            this.isCreateModalOpen = false;
            this.editingProductId = null;
          }
        },
        error: (err) => {
          this.isSaving = false;
          this.cdr.detectChanges();
          const serverMsg = err?.error?.error?.message || err?.message || 'Failed to update product.';
          this.toasterService.error(serverMsg, 'Validation Error');
        }
      });
    } else {
      this.productService.create(this.newProduct).subscribe({
        next: (created) => {
          this.isSaving = false;
          this.toasterService.success('Product registered successfully!', 'Success');
          this.loadProducts();
          if (this.newProduct.productType === ProductType.VariantBased && created.id) {
            this.editingProductId = created.id;
            this.activeDrawerTab = 'variants';
            this.loadProductVariants(created.id);
            this.toasterService.info('Now configure variants and attributes for this product.');
          } else {
            this.isCreateModalOpen = false;
            this.editingProductId = null;
          }
        },
        error: (err) => {
          this.isSaving = false;
          this.cdr.detectChanges();
          const serverMsg = err?.error?.error?.message || err?.message || 'Failed to register product.';
          this.toasterService.error(serverMsg, 'Validation Error');
        }
      });
    }
  }
}
