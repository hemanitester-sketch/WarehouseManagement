import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { PurchaseBillService, PurchaseBillRecord, PurchaseBillItemRecord } from '../services/purchase-bill.service';
import { PurchaseReceiveService, PurchaseReceiveRecord } from '../services/purchase-receive.service';
import { SupplierService } from '../../proxy/suppliers/supplier.service';
import { SupplierLookupDto } from '../../proxy/suppliers/models';
import { WarehouseService } from '../../proxy/warehouses/warehouse.service';
import { WarehouseDto } from '../../proxy/warehouses/models';

@Component({
  selector: 'app-bills',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './bills.component.html',
  styleUrls: ['./bills.component.scss']
})
export class BillsComponent implements OnInit {
  private billService = inject(PurchaseBillService);
  private receiveService = inject(PurchaseReceiveService);
  private supplierService = inject(SupplierService);
  private warehouseService = inject(WarehouseService);
  private toasterService = inject(ToasterService);
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);

  bills: PurchaseBillRecord[] = [];
  receives: PurchaseReceiveRecord[] = [];
  warehouses: WarehouseDto[] = [];
  suppliers: SupplierLookupDto[] = [];

  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedStatusFilter = '';

  // Pagination
  currentPage = 1;
  pageSize = 50;
  Math = Math;

  // Modals
  isCreateBillModalOpen = false;
  isViewBillModalOpen = false;
  selectedBillForView: PurchaseBillRecord | null = null;

  // New Bill Form State
  selectedReceiveId: string = '';
  selectedReceiveForBill: PurchaseReceiveRecord | null = null;
  billVendorInvoiceNumber = '';
  billDateStr = new Date().toISOString().split('T')[0];
  dueDateStr = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
  paymentTerms = 'Net 30';
  freightCharges = 250;
  totalDiscount = 0;
  billNotes = 'Goods receipt verified against vendor bill. Standard GST rates applied.';

  // Line items state
  billLinesState: PurchaseBillItemRecord[] = [];

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadSuppliers();
    this.loadReceives();
    this.loadBills();

    // Check query params if redirected from GRN page
    this.route.queryParams.subscribe(params => {
      if (params['grnId']) {
        setTimeout(() => {
          this.openCreateBillModal(params['grnId']);
        }, 300);
      }
    });
  }

  loadWarehouses(): void {
    this.warehouseService.getList({ maxResultCount: 50, skipCount: 0 }).subscribe({
      next: (res) => {
        this.warehouses = res.items || [];
        this.cdr.detectChanges();
      }
    });
  }

  loadSuppliers(): void {
    this.supplierService.getLookup().subscribe({
      next: (list: any) => {
        this.suppliers = (list && list.items) || list || [];
        this.cdr.detectChanges();
      }
    });
  }

  loadReceives(): void {
    this.receiveService.getReceives().subscribe({
      next: (list) => {
        this.receives = list || [];
        this.cdr.detectChanges();
      }
    });
  }

  loadBills(): void {
    this.isLoading = true;
    this.billService.getBills().subscribe({
      next: (list) => {
        this.bills = list || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Filtered Bills
  get filteredBills(): PurchaseBillRecord[] {
    let list = this.bills;

    if (this.selectedStatusFilter) {
      list = list.filter(b => b.status === this.selectedStatusFilter);
    }

    if (this.searchQuery && this.searchQuery.trim()) {
      const q = this.searchQuery.trim().toLowerCase();
      list = list.filter(b => 
        (b.billNumber && b.billNumber.toLowerCase().includes(q)) ||
        (b.vendorInvoiceNumber && b.vendorInvoiceNumber.toLowerCase().includes(q)) ||
        (b.orderNumber && b.orderNumber.toLowerCase().includes(q)) ||
        (b.receiveNumber && b.receiveNumber.toLowerCase().includes(q)) ||
        (b.supplierName && b.supplierName.toLowerCase().includes(q))
      );
    }

    return list;
  }

  get paginatedBills(): PurchaseBillRecord[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredBills.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredBills.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    if (this.filteredBills.length === 0) return 0;
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredBills.length);
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
  get totalBillsCount(): number {
    return this.bills.length;
  }

  get totalBilledAmount(): number {
    return this.bills.reduce((sum, b) => sum + (b.grandTotal || 0), 0);
  }

  get totalBalanceDue(): number {
    return this.bills.reduce((sum, b) => sum + (b.balanceDue || 0), 0);
  }

  get totalPaidAmount(): number {
    return this.bills.reduce((sum, b) => sum + (b.paidAmount || 0), 0);
  }

  // Modal: Open Create Bill
  openCreateBillModal(preselectedGrnId?: string): void {
    const today = new Date();
    this.billDateStr = today.toISOString().split('T')[0];
    this.paymentTerms = 'Net 30';
    this.calculateDueDate();
    this.freightCharges = 250;
    this.totalDiscount = 0;
    this.billNotes = 'Goods receipt verified against vendor bill. Standard GST rates applied.';

    if (preselectedGrnId) {
      this.selectedReceiveId = preselectedGrnId;
    } else if (this.receives.length > 0) {
      this.selectedReceiveId = this.receives[0].id;
    } else {
      this.selectedReceiveId = '';
    }

    this.onReceiveSelected();
    this.isCreateBillModalOpen = true;
  }

  calculateDueDate(): void {
    const baseDate = new Date(this.billDateStr || Date.now());
    let days = 30;
    if (this.paymentTerms === 'Net 15') days = 15;
    else if (this.paymentTerms === 'Net 60') days = 60;
    else if (this.paymentTerms === 'Immediate' || this.paymentTerms === 'Advance') days = 0;
    const due = new Date(baseDate.getTime() + days * 24 * 60 * 60 * 1000);
    this.dueDateStr = due.toISOString().split('T')[0];
  }

  onReceiveSelected(): void {
    if (!this.selectedReceiveId) {
      this.selectedReceiveForBill = null;
      this.billLinesState = [];
      return;
    }

    this.selectedReceiveForBill = this.receives.find(r => r.id === this.selectedReceiveId || r.receiveNumber === this.selectedReceiveId) || null;
    if (this.selectedReceiveForBill) {
      this.billVendorInvoiceNumber = this.selectedReceiveForBill.supplierInvoiceNumber || `INV-${new Date().getFullYear()}-${Math.floor(1000 + Math.random() * 9000)}`;

      this.billLinesState = (this.selectedReceiveForBill.items || []).map(item => {
        const qty = item.quantityAccepted || item.quantityReceived || 1;
        const rate = item.unitPrice > 0 ? item.unitPrice : 150;
        const discPercent = 0;
        const discAmt = 0;
        const taxable = (qty * rate) - discAmt;
        const taxRate = 18; // default 18% GST
        const taxAmt = Math.round((taxable * taxRate / 100) * 100) / 100;
        return {
          productId: item.productId,
          productSku: item.productSku,
          productName: item.productName,
          productVariantId: item.productVariantId,
          variantSku: item.variantSku,
          variantName: item.variantName,
          hsnCode: '3004',
          quantity: qty,
          unitPrice: rate,
          discountPercent: discPercent,
          discountAmount: discAmt,
          taxRatePercent: taxRate,
          taxAmount: taxAmt,
          lineTotal: taxable + taxAmt
        };
      });
    }
  }

  onLinePriceOrQtyChanged(line: PurchaseBillItemRecord): void {
    const taxable = (line.quantity * line.unitPrice) - (line.discountAmount || 0);
    line.taxAmount = Math.round((taxable * (line.taxRatePercent || 0) / 100) * 100) / 100;
    line.lineTotal = Math.round((taxable + line.taxAmount) * 100) / 100;
  }

  // Summary Calculations
  get calculatedSubTotal(): number {
    return this.billLinesState.reduce((sum, l) => sum + (l.quantity * l.unitPrice), 0);
  }

  get calculatedTaxTotal(): number {
    return this.billLinesState.reduce((sum, l) => sum + (l.taxAmount || 0), 0);
  }

  get calculatedGrandTotal(): number {
    const sub = this.calculatedSubTotal;
    const disc = this.totalDiscount || 0;
    const freight = this.freightCharges || 0;
    const tax = this.calculatedTaxTotal;
    return Math.round(sub - disc + freight + tax);
  }

  submitBill(): void {
    if (!this.selectedReceiveForBill) {
      this.toasterService.warn('Please select a Goods Receipt Note (GRN) to bill.');
      return;
    }

    if (!this.billVendorInvoiceNumber.trim()) {
      this.toasterService.warn('Please enter the Vendor Invoice Number.');
      return;
    }

    if (this.billLinesState.length === 0) {
      this.toasterService.warn('No items found in this Goods Receipt to bill.');
      return;
    }

    this.isSaving = true;

    const sub = this.calculatedSubTotal;
    const tax = this.calculatedTaxTotal;
    const freight = this.freightCharges || 0;
    const grand = this.calculatedGrandTotal;

    const newBill = this.billService.recordBill({
      vendorInvoiceNumber: this.billVendorInvoiceNumber.trim(),
      inboundOrderId: this.selectedReceiveForBill.inboundOrderId,
      orderNumber: this.selectedReceiveForBill.orderNumber,
      receiveId: this.selectedReceiveForBill.id,
      receiveNumber: this.selectedReceiveForBill.receiveNumber,
      supplierName: this.selectedReceiveForBill.supplierName,
      supplierGstin: '07AABCA5678Q1Z2',
      supplierAddress: 'Plot 42, Industrial Area, Sector 62',
      warehouseId: this.selectedReceiveForBill.warehouseId,
      warehouseName: this.selectedReceiveForBill.warehouseName,
      warehouseCode: this.selectedReceiveForBill.warehouseCode,
      billDate: new Date(this.billDateStr).toISOString(),
      dueDate: new Date(this.dueDateStr).toISOString(),
      paymentTerms: this.paymentTerms,
      status: 'Open',
      subTotal: sub,
      totalDiscount: this.totalDiscount || 0,
      freightCharges: freight,
      totalTax: tax,
      cgstAmount: Math.round(tax / 2 * 100) / 100,
      sgstAmount: Math.round(tax / 2 * 100) / 100,
      igstAmount: 0,
      roundOff: 0,
      grandTotal: grand,
      paidAmount: 0,
      balanceDue: grand,
      notes: this.billNotes,
      items: this.billLinesState
    });

    this.isSaving = false;
    this.isCreateBillModalOpen = false;
    this.toasterService.success(`Purchase Bill ${newBill.billNumber} created successfully!`, 'Bill Generated');
    this.loadBills();

    // Open View document immediately
    this.viewBill(newBill);
  }

  viewBill(bill: PurchaseBillRecord): void {
    this.selectedBillForView = bill;
    this.isViewBillModalOpen = true;
  }

  printBill(): void {
    window.print();
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Paid': return 'wms-badge-success';
      case 'Partially Paid': return 'wms-badge-info';
      case 'Overdue': return 'wms-badge-danger';
      default: return 'wms-badge-warning';
    }
  }

  // Convert numbers to words for GST invoice
  numberToWords(num: number): string {
    const a = ['', 'One ', 'Two ', 'Three ', 'Four ', 'Five ', 'Six ', 'Seven ', 'Eight ', 'Nine ', 'Ten ', 'Eleven ', 'Twelve ', 'Thirteen ', 'Fourteen ', 'Fifteen ', 'Sixteen ', 'Seventeen ', 'Eighteen ', 'Nineteen '];
    const b = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];

    const n = ('000000000' + num).substr(-9).match(/^(\d{2})(\d{2})(\d{2})(\d{1})(\d{2})$/);
    if (!n) return '';
    let str = '';
    str += (n[1] != '00') ? (a[Number(n[1])] || b[Number(n[1][0])] + ' ' + a[Number(n[1][1])]) + 'Crore ' : '';
    str += (n[2] != '00') ? (a[Number(n[2])] || b[Number(n[2][0])] + ' ' + a[Number(n[2][1])]) + 'Lakh ' : '';
    str += (n[3] != '00') ? (a[Number(n[3])] || b[Number(n[3][0])] + ' ' + a[Number(n[3][1])]) + 'Thousand ' : '';
    str += (n[4] != '0') ? (a[Number(n[4])] || b[Number(n[4][0])] + ' ' + a[Number(n[4][1])]) + 'Hundred ' : '';
    str += (n[5] != '00') ? ((str != '') ? 'and ' : '') + (a[Number(n[5])] || b[Number(n[5][0])] + ' ' + a[Number(n[5][1])]) : '';
    return str.trim() ? `${str.trim()} Rupees Only` : 'Zero Rupees';
  }
}
