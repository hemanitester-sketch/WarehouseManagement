import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService, ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { CustomerService } from '../proxy/customers/customer.service';
import {
  CustomerDto,
  CreateUpdateCustomerDto,
  CustomerDashboardMetricsDto,
  CustomerType,
  CustomerStatus,
} from '../proxy/customers/models';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.scss'],
})
export class CustomersComponent implements OnInit {
  private customerService = inject(CustomerService);
  private toasterService = inject(ToasterService);
  private confirmationService = inject(ConfirmationService);
  private cdr = inject(ChangeDetectorRef);

  customers: CustomerDto[] = [];
  filteredCustomers: CustomerDto[] = [];

  metrics: CustomerDashboardMetricsDto = {
    totalClientsCount: 0,
    activeClientsCount: 0,
    corporateB2BCount: 0,
    retailDistributorCount: 0,
    totalCreditLimitExposure: 0,
    totalCurrentOutstanding: 0,
    averageCreditUtilizationPercent: 0,
    onHoldClientsCount: 0,
  };

  isLoading = false;
  isSaving = false;

  // Search & Filters
  searchQuery = '';
  selectedTypeFilter = 'all';
  selectedStatusFilter = 'all';
  selectedCityFilter = 'all';
  selectedTierFilter = 'all';

  availableCities: string[] = [];
  availableTiers: string[] = [];

  // Enums for template access
  CustomerType = CustomerType;
  CustomerStatus = CustomerStatus;

  // Pagination (50 default limit)
  currentPage = 1;
  pageSize = 50;

  get paginatedCustomers(): CustomerDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredCustomers.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredCustomers.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    return this.filteredCustomers.length === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredCustomers.length);
  }

  // Modals & Offcanvas State
  isFormModalOpen = false;
  isViewModalOpen = false;
  isEditing = false;
  editingId: string | null = null;
  formModalTitle = 'Register New Enterprise Client';

  selectedCustomer: CustomerDto | null = null;

  // Active tab in Add/Edit form
  activeFormTab: 'general' | 'contact' | 'statutory' | 'commercial' = 'general';

  // Form State
  formModel: CreateUpdateCustomerDto = this.getEmptyForm();

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.customerService.getList({ maxResultCount: 200, skipCount: 0 }).subscribe({
      next: res => {
        this.customers = res.items || [];
        this.extractUniqueFilters();
        this.applyFilters();
        this.loadMetrics();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.isLoading = false;
        this.toasterService.error(err?.error?.message || 'Failed to load customer directory.');
        this.cdr.detectChanges();
      },
    });
  }

  loadMetrics(): void {
    this.customerService.getDashboardMetrics().subscribe({
      next: metrics => {
        this.metrics = metrics;
        this.cdr.detectChanges();
      },
      error: () => {},
    });
  }

  extractUniqueFilters(): void {
    const citiesSet = new Set<string>();
    const tiersSet = new Set<string>();

    for (const c of this.customers) {
      if (c.city && c.city.trim()) {
        citiesSet.add(c.city.trim());
      }
      if (c.priceTier && c.priceTier.trim()) {
        tiersSet.add(c.priceTier.trim());
      }
    }

    this.availableCities = Array.from(citiesSet).sort();
    this.availableTiers = Array.from(tiersSet).sort();
  }

  applyFilters(): void {
    let result = [...this.customers];

    // Text Search
    if (this.searchQuery && this.searchQuery.trim()) {
      const q = this.searchQuery.trim().toLowerCase();
      result = result.filter(
        c =>
          c.code.toLowerCase().includes(q) ||
          c.name.toLowerCase().includes(q) ||
          (c.contactPerson && c.contactPerson.toLowerCase().includes(q)) ||
          (c.phoneNumber && c.phoneNumber.toLowerCase().includes(q)) ||
          (c.email && c.email.toLowerCase().includes(q)) ||
          (c.city && c.city.toLowerCase().includes(q)) ||
          (c.taxNumber && c.taxNumber.toLowerCase().includes(q)) ||
          (c.industry && c.industry.toLowerCase().includes(q))
      );
    }

    // Type Filter
    if (this.selectedTypeFilter !== 'all') {
      const typeNum = Number(this.selectedTypeFilter);
      result = result.filter(c => c.customerType === typeNum);
    }

    // Status Filter
    if (this.selectedStatusFilter !== 'all') {
      const statusNum = Number(this.selectedStatusFilter);
      result = result.filter(c => c.status === statusNum);
    }

    // City Filter
    if (this.selectedCityFilter !== 'all') {
      result = result.filter(c => c.city === this.selectedCityFilter);
    }

    // Tier Filter
    if (this.selectedTierFilter !== 'all') {
      result = result.filter(c => c.priceTier === this.selectedTierFilter);
    }

    this.filteredCustomers = result;
    this.currentPage = 1;
    this.cdr.detectChanges();
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.selectedTypeFilter = 'all';
    this.selectedStatusFilter = 'all';
    this.selectedCityFilter = 'all';
    this.selectedTierFilter = 'all';
    this.applyFilters();
  }

  // Pagination navigation
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

  // Add / Edit Modal Controls
  openCreateModal(): void {
    this.isEditing = false;
    this.editingId = null;
    this.formModalTitle = 'Register New Enterprise Client';
    this.activeFormTab = 'general';
    this.formModel = this.getEmptyForm();

    // Auto-generate next candidate Code
    this.formModel.code = this.generateNextCode();
    this.isFormModalOpen = true;
  }

  openEditModal(customer: CustomerDto): void {
    this.isEditing = true;
    this.editingId = customer.id || null;
    this.formModalTitle = `Edit Client: ${customer.name} (${customer.code})`;
    this.activeFormTab = 'general';

    this.formModel = {
      code: customer.code,
      name: customer.name,
      customerType: customer.customerType,
      industry: customer.industry || '',
      contactPerson: customer.contactPerson || '',
      email: customer.email || '',
      phoneNumber: customer.phoneNumber || '',
      alternativePhone: customer.alternativePhone || '',
      taxNumber: customer.taxNumber || '',
      panNumber: customer.panNumber || '',
      billingAddress: customer.billingAddress || '',
      shippingAddress: customer.shippingAddress || '',
      city: customer.city || '',
      state: customer.state || '',
      postalCode: customer.postalCode || '',
      country: customer.country || 'India',
      paymentTerms: customer.paymentTerms || 'Net 30',
      creditLimit: customer.creditLimit || 500000,
      currentOutstanding: customer.currentOutstanding || 0,
      priceTier: customer.priceTier || 'Tier 2 (Standard B2B)',
      deliveryDockHours: customer.deliveryDockHours || '',
      specialInstructions: customer.specialInstructions || '',
      status: customer.status,
      isActive: customer.isActive,
      rating: customer.rating || 5,
      notes: customer.notes || '',
    };

    this.isFormModalOpen = true;
  }

  closeFormModal(): void {
    this.isFormModalOpen = false;
    this.isEditing = false;
    this.editingId = null;
  }

  saveCustomer(): void {
    if (!this.formModel.code || !this.formModel.code.trim()) {
      this.toasterService.warn('Client Code is required.');
      return;
    }

    if (!this.formModel.name || !this.formModel.name.trim()) {
      this.toasterService.warn('Client Legal / Trade Name is required.');
      return;
    }

    this.isSaving = true;

    if (this.isEditing && this.editingId) {
      this.customerService.update(this.editingId, this.formModel).subscribe({
        next: updated => {
          this.isSaving = false;
          this.toasterService.success(`Client '${updated.name}' updated successfully.`);
          this.closeFormModal();
          this.loadData();
        },
        error: err => {
          this.isSaving = false;
          this.toasterService.error(err?.error?.message || 'Failed to update client.');
          this.cdr.detectChanges();
        },
      });
    } else {
      this.customerService.create(this.formModel).subscribe({
        next: created => {
          this.isSaving = false;
          this.toasterService.success(`Client '${created.name}' registered successfully.`);
          this.closeFormModal();
          this.loadData();
        },
        error: err => {
          this.isSaving = false;
          this.toasterService.error(err?.error?.message || 'Failed to create client.');
          this.cdr.detectChanges();
        },
      });
    }
  }

  // View Customer Dossier
  openViewModal(customer: CustomerDto): void {
    this.selectedCustomer = customer;
    this.isViewModalOpen = true;
  }

  closeViewModal(): void {
    this.isViewModalOpen = false;
    this.selectedCustomer = null;
  }

  // Quick Status Toggle (Hold / Active)
  toggleClientHold(customer: CustomerDto): void {
    const newStatus =
      customer.status === CustomerStatus.OnHold ? CustomerStatus.Active : CustomerStatus.OnHold;
    const actionText = newStatus === CustomerStatus.OnHold ? 'place on HOLD' : 'reactivate';

    this.confirmationService
      .warn(
        `Are you sure you want to ${actionText} client account '${customer.name}'?`,
        'Confirm Account Status Change'
      )
      .subscribe((status: Confirmation.Status) => {
        if (status === Confirmation.Status.confirm && customer.id) {
          this.customerService.toggleStatus(customer.id, newStatus).subscribe({
            next: updated => {
              this.toasterService.success(`Client account is now ${updated.statusName}.`);
              this.loadData();
            },
            error: err => {
              this.toasterService.error(err?.error?.message || 'Failed to update account status.');
            },
          });
        }
      });
  }

  // Delete Customer
  deleteCustomer(customer: CustomerDto): void {
    this.confirmationService
      .warn(
        `Are you sure you want to delete client '${customer.name}' (${customer.code})? This action cannot be undone.`,
        'Confirm Client Deletion'
      )
      .subscribe((status: Confirmation.Status) => {
        if (status === Confirmation.Status.confirm && customer.id) {
          this.customerService.delete(customer.id).subscribe({
            next: () => {
              this.toasterService.success(`Client '${customer.name}' deleted.`);
              this.loadData();
            },
            error: err => {
              this.toasterService.error(err?.error?.message || 'Failed to delete client.');
            },
          });
        }
      });
  }

  // Export to CSV
  exportToCsv(): void {
    if (this.filteredCustomers.length === 0) {
      this.toasterService.info('No clients to export.');
      return;
    }

    const headers = [
      'Client Code',
      'Company Name',
      'Customer Type',
      'Industry',
      'Contact Person',
      'Phone',
      'Email',
      'GSTIN',
      'City',
      'State',
      'Payment Terms',
      'Credit Limit',
      'Current Outstanding',
      'Price Tier',
      'Status',
    ];

    const rows = this.filteredCustomers.map(c => [
      `"${c.code}"`,
      `"${c.name.replace(/"/g, '""')}"`,
      `"${c.customerTypeName}"`,
      `"${c.industry || ''}"`,
      `"${c.contactPerson || ''}"`,
      `"${c.phoneNumber || ''}"`,
      `"${c.email || ''}"`,
      `"${c.taxNumber || ''}"`,
      `"${c.city || ''}"`,
      `"${c.state || ''}"`,
      `"${c.paymentTerms || ''}"`,
      `"${c.creditLimit}"`,
      `"${c.currentOutstanding}"`,
      `"${c.priceTier || ''}"`,
      `"${c.statusName}"`,
    ]);

    const csvContent = '\uFEFF' + [headers.join(','), ...rows.map(r => r.join(','))].join('\r\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', `WareFlow_Customers_Export_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    this.toasterService.success('Customer directory exported successfully.');
  }

  // Helpers
  generateNextCode(): string {
    const maxNumber = this.customers.reduce((max, c) => {
      const match = c.code.match(/CUST-(\d+)/i);
      if (match && match[1]) {
        const num = parseInt(match[1], 10);
        return num > max ? num : max;
      }
      return max;
    }, 1000);

    return `CUST-${maxNumber + 1}`;
  }

  getTypeBadgeClass(type: CustomerType): string {
    switch (type) {
      case CustomerType.CorporateB2B:
        return 'type-corporate';
      case CustomerType.RetailB2C:
        return 'type-retail';
      case CustomerType.Distributor:
        return 'type-distributor';
      case CustomerType.EcommercePartner:
        return 'type-ecommerce';
      case CustomerType.Government:
        return 'type-government';
      default:
        return 'type-default';
    }
  }

  getStatusBadgeClass(status: CustomerStatus): string {
    switch (status) {
      case CustomerStatus.Active:
        return 'status-active';
      case CustomerStatus.UnderReview:
        return 'status-review';
      case CustomerStatus.OnHold:
        return 'status-hold';
      case CustomerStatus.Inactive:
        return 'status-inactive';
      default:
        return 'status-default';
    }
  }

  getStarArray(rating?: number): number[] {
    const count = rating && rating > 0 ? Math.min(5, rating) : 5;
    return Array(count).fill(0);
  }

  private getEmptyForm(): CreateUpdateCustomerDto {
    return {
      code: '',
      name: '',
      customerType: CustomerType.CorporateB2B,
      industry: 'Consumer Goods & Retail',
      contactPerson: '',
      email: '',
      phoneNumber: '',
      alternativePhone: '',
      taxNumber: '',
      panNumber: '',
      billingAddress: '',
      shippingAddress: '',
      city: '',
      state: '',
      postalCode: '',
      country: 'India',
      paymentTerms: 'Net 30',
      creditLimit: 500000,
      currentOutstanding: 0,
      priceTier: 'Tier 2 (Standard B2B)',
      deliveryDockHours: '09:00 AM - 06:00 PM (Mon-Sat)',
      specialInstructions: '',
      status: CustomerStatus.Active,
      isActive: true,
      rating: 5,
      notes: '',
    };
  }
}
