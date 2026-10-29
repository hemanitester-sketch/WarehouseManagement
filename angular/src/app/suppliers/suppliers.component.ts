import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { SupplierService } from '../proxy/suppliers/supplier.service';
import { SupplierDto, CreateUpdateSupplierDto } from '../proxy/suppliers/models';

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './suppliers.component.html',
  styleUrls: ['./suppliers.component.scss'],
})
export class SuppliersComponent implements OnInit {
  private supplierService = inject(SupplierService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  suppliers: SupplierDto[] = [];
  filteredSuppliers: SupplierDto[] = [];

  isLoading = false;
  isSaving = false;

  // Filters
  searchQuery = '';
  selectedStatusFilter = 'all';
  selectedPaymentTermsFilter = 'all';
  selectedCityFilter = 'all';

  // Available unique cities for filter
  availableCities: string[] = [];

  // Pagination (50 default limit)
  currentPage = 1;
  pageSize = 50;

  get paginatedSuppliers(): SupplierDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredSuppliers.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredSuppliers.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    return this.filteredSuppliers.length === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredSuppliers.length);
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

  // KPI Metrics
  totalSuppliersCount = 0;
  activeSuppliersCount = 0;
  gstRegisteredCount = 0;
  inactiveSuppliersCount = 0;

  // Drawer / Modal State
  isDrawerOpen = false;
  isEditing = false;
  editingId: string | null = null;
  drawerTitle = 'Register New Supplier & Vendor';

  // Payment Terms Presets
  paymentTermsPresets: string[] = [
    'Net 30',
    'Net 15',
    'Net 45',
    'Net 60',
    'Advance Payment (100%)',
    '50% Advance, 50% on Delivery',
    'Immediate / COD (Cash on Delivery)',
    'Letter of Credit (LC)',
  ];

  // Form Model
  formModel: CreateUpdateSupplierDto = this.getInitialFormModel();

  ngOnInit(): void {
    this.loadSuppliers();
  }

  getInitialFormModel(): CreateUpdateSupplierDto {
    const randomCode = `SUP-${new Date().getFullYear()}-${Math.floor(100 + Math.random() * 900)}`;
    return {
      code: randomCode,
      name: '',
      contactPerson: '',
      email: '',
      phoneNumber: '',
      taxNumber: '',
      address: '',
      city: '',
      state: '',
      postalCode: '',
      paymentTerms: 'Net 30',
      isActive: true,
      rating: 5,
      notes: '',
    };
  }

  loadSuppliers(): void {
    this.isLoading = true;
    this.supplierService
      .getList({
        maxResultCount: 1000,
        skipCount: 0,
      })
      .subscribe({
        next: (res) => {
          this.suppliers = res.items || [];
          this.extractAvailableCities();
          this.calculateMetrics();
          this.applyFilters();
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.isLoading = false;
          this.toasterService.error(
            err.error?.error?.message || 'Failed to load suppliers. Please try again.',
            'Error'
          );
          this.cdr.detectChanges();
        },
      });
  }

  extractAvailableCities(): void {
    const citiesSet = new Set<string>();
    this.suppliers.forEach((s) => {
      if (s.city && s.city.trim().length > 0) {
        citiesSet.add(s.city.trim());
      }
    });
    this.availableCities = Array.from(citiesSet).sort();
  }

  calculateMetrics(): void {
    this.totalSuppliersCount = this.suppliers.length;
    this.activeSuppliersCount = this.suppliers.filter((s) => s.isActive).length;
    this.gstRegisteredCount = this.suppliers.filter(
      (s) => s.taxNumber && s.taxNumber.trim().length > 0
    ).length;
    this.inactiveSuppliersCount = this.suppliers.filter((s) => !s.isActive).length;
  }

  applyFilters(): void {
    const q = this.searchQuery.trim().toLowerCase();

    this.filteredSuppliers = this.suppliers.filter((supplier) => {
      // Search matching
      const matchesSearch =
        !q ||
        supplier.code?.toLowerCase().includes(q) ||
        supplier.name?.toLowerCase().includes(q) ||
        supplier.contactPerson?.toLowerCase().includes(q) ||
        supplier.phoneNumber?.toLowerCase().includes(q) ||
        supplier.email?.toLowerCase().includes(q) ||
        supplier.taxNumber?.toLowerCase().includes(q) ||
        supplier.city?.toLowerCase().includes(q) ||
        supplier.state?.toLowerCase().includes(q);

      // Status filter
      let matchesStatus = true;
      if (this.selectedStatusFilter === 'active') {
        matchesStatus = supplier.isActive === true;
      } else if (this.selectedStatusFilter === 'inactive') {
        matchesStatus = supplier.isActive === false;
      }

      // City filter
      let matchesCity = true;
      if (this.selectedCityFilter !== 'all') {
        matchesCity = supplier.city?.toLowerCase() === this.selectedCityFilter.toLowerCase();
      }

      // Payment Terms filter
      let matchesTerms = true;
      if (this.selectedPaymentTermsFilter !== 'all') {
        matchesTerms = supplier.paymentTerms === this.selectedPaymentTermsFilter;
      }

      return matchesSearch && matchesStatus && matchesCity && matchesTerms;
    });

    this.currentPage = 1;
    this.cdr.detectChanges();
  }

  resetFilters(): void {
    this.searchQuery = '';
    this.selectedStatusFilter = 'all';
    this.selectedCityFilter = 'all';
    this.selectedPaymentTermsFilter = 'all';
    this.applyFilters();
  }

  openCreateDrawer(): void {
    this.isEditing = false;
    this.editingId = null;
    this.drawerTitle = 'Register New Supplier & Vendor';
    this.formModel = this.getInitialFormModel();
    this.isDrawerOpen = true;
  }

  openEditDrawer(supplier: SupplierDto): void {
    this.isEditing = true;
    this.editingId = supplier.id || null;
    this.drawerTitle = `Edit Supplier: ${supplier.name}`;
    this.formModel = {
      code: supplier.code,
      name: supplier.name,
      contactPerson: supplier.contactPerson || '',
      email: supplier.email || '',
      phoneNumber: supplier.phoneNumber || '',
      taxNumber: supplier.taxNumber || '',
      address: supplier.address || '',
      city: supplier.city || '',
      state: supplier.state || '',
      postalCode: supplier.postalCode || '',
      paymentTerms: supplier.paymentTerms || 'Net 30',
      isActive: supplier.isActive,
      rating: supplier.rating || 5,
      notes: supplier.notes || '',
    };
    this.isDrawerOpen = true;
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
    this.editingId = null;
    this.isSaving = false;
  }

  saveSupplier(): void {
    if (!this.formModel.code || !this.formModel.code.trim()) {
      this.toasterService.warn('Supplier Code is required.', 'Validation');
      return;
    }

    if (!this.formModel.name || !this.formModel.name.trim()) {
      this.toasterService.warn('Supplier Name is required.', 'Validation');
      return;
    }

    this.isSaving = true;

    if (this.isEditing && this.editingId) {
      this.supplierService.update(this.editingId, this.formModel).subscribe({
        next: (updated) => {
          this.isSaving = false;
          this.toasterService.success(
            `Supplier '${updated.name}' updated successfully!`,
            'Updated'
          );
          this.closeDrawer();
          this.loadSuppliers();
        },
        error: (err) => {
          this.isSaving = false;
          this.toasterService.error(
            err.error?.error?.message || 'Failed to update supplier.',
            'Error'
          );
        },
      });
    } else {
      this.supplierService.create(this.formModel).subscribe({
        next: (created) => {
          this.isSaving = false;
          this.toasterService.success(
            `Supplier '${created.name}' registered successfully!`,
            'Created'
          );
          this.closeDrawer();
          this.loadSuppliers();
        },
        error: (err) => {
          this.isSaving = false;
          this.toasterService.error(
            err.error?.error?.message || 'Failed to register supplier.',
            'Error'
          );
        },
      });
    }
  }

  deleteSupplier(supplier: SupplierDto): void {
    if (!supplier.id) {
      return;
    }
    if (confirm(`Are you sure you want to delete supplier '${supplier.name}' (${supplier.code})?`)) {
      this.supplierService.delete(supplier.id).subscribe({
        next: () => {
          this.toasterService.success(
            `Supplier '${supplier.name}' was deleted successfully.`,
            'Deleted'
          );
          this.loadSuppliers();
        },
        error: (err) => {
          this.toasterService.error(
            err.error?.error?.message || 'Failed to delete supplier.',
            'Error'
          );
        },
      });
    }
  }

  getRatingArray(rating: number | undefined): boolean[] {
    const r = rating || 5;
    return [1, 2, 3, 4, 5].map((i) => i <= r);
  }
}
