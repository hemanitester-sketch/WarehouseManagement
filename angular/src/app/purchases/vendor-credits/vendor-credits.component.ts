import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { VendorCreditService, VendorCreditRecord, VendorCreditItemRecord } from '../services/vendor-credit.service';
import { PurchaseReceiveService, PurchaseReceiveRecord } from '../services/purchase-receive.service';
import { PurchaseBillService, PurchaseBillRecord } from '../services/purchase-bill.service';

@Component({
  selector: 'app-vendor-credits',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './vendor-credits.component.html',
  styleUrls: ['./vendor-credits.component.scss']
})
export class VendorCreditsComponent implements OnInit {
  private creditService = inject(VendorCreditService);
  private receiveService = inject(PurchaseReceiveService);
  private billService = inject(PurchaseBillService);
  private toasterService = inject(ToasterService);
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);

  credits: VendorCreditRecord[] = [];
  receives: PurchaseReceiveRecord[] = [];
  openBills: PurchaseBillRecord[] = [];

  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedStatusFilter = '';

  // Pagination
  currentPage = 1;
  pageSize = 50;
  Math = Math;

  // Modals
  isCreateCreditModalOpen = false;
  isApplyCreditModalOpen = false;
  isViewCreditModalOpen = false;

  selectedCreditForView: VendorCreditRecord | null = null;
  selectedCreditForApply: VendorCreditRecord | null = null;

  // Create Credit Form State
  selectedReceiveId: string = '';
  selectedReceiveForCredit: PurchaseReceiveRecord | null = null;
  creditReasonType: VendorCreditRecord['reasonType'] = 'Damaged in Transit';
  creditDateStr = new Date().toISOString().split('T')[0];
  creditNotes = 'Credit claim raised for damaged/rejected goods identified during dock inspection.';
  creditLinesState: VendorCreditItemRecord[] = [];

  // Apply Credit Form State
  targetBillId = '';
  amountToAdjust = 0;

  ngOnInit(): void {
    this.loadReceives();
    this.loadBills();
    this.loadCredits();

    // Check query params if redirected from GRN page
    this.route.queryParams.subscribe(params => {
      if (params['grnId']) {
        setTimeout(() => {
          this.openCreateCreditModal(params['grnId']);
        }, 300);
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
    this.billService.getBills().subscribe({
      next: (list) => {
        this.openBills = (list || []).filter(b => (b.balanceDue || 0) > 0);
        this.cdr.detectChanges();
      }
    });
  }

  loadCredits(): void {
    this.isLoading = true;
    this.creditService.getCredits().subscribe({
      next: (list) => {
        this.credits = list || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Filtered Credits
  get filteredCredits(): VendorCreditRecord[] {
    let list = this.credits;

    if (this.selectedStatusFilter) {
      list = list.filter(c => c.status === this.selectedStatusFilter);
    }

    if (this.searchQuery && this.searchQuery.trim()) {
      const q = this.searchQuery.trim().toLowerCase();
      list = list.filter(c => 
        (c.creditNumber && c.creditNumber.toLowerCase().includes(q)) ||
        (c.supplierName && c.supplierName.toLowerCase().includes(q)) ||
        (c.orderNumber && c.orderNumber.toLowerCase().includes(q)) ||
        (c.receiveNumber && c.receiveNumber.toLowerCase().includes(q)) ||
        (c.reasonType && c.reasonType.toLowerCase().includes(q))
      );
    }

    return list;
  }

  get paginatedCredits(): VendorCreditRecord[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredCredits.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredCredits.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    if (this.filteredCredits.length === 0) return 0;
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredCredits.length);
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
  get totalCreditsCount(): number {
    return this.credits.length;
  }

  get totalCreditAmount(): number {
    return this.credits.reduce((sum, c) => sum + (c.totalCreditAmount || 0), 0);
  }

  get totalAvailableBalance(): number {
    return this.credits.reduce((sum, c) => sum + (c.availableBalance || 0), 0);
  }

  get totalAdjustedAmount(): number {
    return this.credits.reduce((sum, c) => sum + (c.adjustedAmount || 0), 0);
  }

  // Modal: Open Create Credit
  openCreateCreditModal(preselectedGrnId?: string): void {
    this.creditDateStr = new Date().toISOString().split('T')[0];
    this.creditReasonType = 'Damaged in Transit';
    this.creditNotes = 'Credit claim raised for damaged/rejected goods identified during dock inspection.';

    if (preselectedGrnId) {
      this.selectedReceiveId = preselectedGrnId;
    } else {
      const damagedGrn = this.receives.find(r => (r.totalDamagedQuantity || 0) > 0);
      this.selectedReceiveId = damagedGrn ? damagedGrn.id : (this.receives.length > 0 ? this.receives[0].id : '');
    }

    this.onReceiveSelected();
    this.isCreateCreditModalOpen = true;
  }

  onReceiveSelected(): void {
    if (!this.selectedReceiveId) {
      this.selectedReceiveForCredit = null;
      this.creditLinesState = [];
      return;
    }

    this.selectedReceiveForCredit = this.receives.find(r => r.id === this.selectedReceiveId || r.receiveNumber === this.selectedReceiveId) || null;
    if (this.selectedReceiveForCredit) {
      const items = (this.selectedReceiveForCredit.items || []).filter(i => (i.quantityDamaged || 0) > 0);
      const sourceList = items.length > 0 ? items : (this.selectedReceiveForCredit.items || []);

      this.creditLinesState = sourceList.map(item => {
        const qty = item.quantityDamaged > 0 ? item.quantityDamaged : 1;
        const rate = item.unitPrice > 0 ? item.unitPrice : 150;
        const taxable = qty * rate;
        const tax = Math.round((taxable * 0.18) * 100) / 100;
        return {
          productId: item.productId,
          productSku: item.productSku,
          productName: item.productName,
          quantityDamagedOrReturned: qty,
          unitPrice: rate,
          taxRatePercent: 18,
          taxAmount: tax,
          claimAmount: taxable + tax,
          reason: item.rejectionReason || 'Damaged packaging during transit'
        };
      });
    }
  }

  onLineChanged(line: VendorCreditItemRecord): void {
    const taxable = line.quantityDamagedOrReturned * line.unitPrice;
    line.taxAmount = Math.round((taxable * (line.taxRatePercent / 100)) * 100) / 100;
    line.claimAmount = Math.round((taxable + line.taxAmount) * 100) / 100;
  }

  get totalCalculatedClaim(): number {
    return this.creditLinesState.reduce((sum, l) => sum + (l.claimAmount || 0), 0);
  }

  submitCredit(): void {
    if (!this.selectedReceiveForCredit) {
      this.toasterService.warn('Please select a Goods Receipt Note (GRN).');
      return;
    }

    if (this.creditLinesState.length === 0) {
      this.toasterService.warn('Please add at least one line item to claim credit.');
      return;
    }

    this.isSaving = true;
    const totalClaim = this.totalCalculatedClaim;

    const newCredit = this.creditService.recordCredit({
      supplierName: this.selectedReceiveForCredit.supplierName,
      supplierGstin: '07AABCA5678Q1Z2',
      orderNumber: this.selectedReceiveForCredit.orderNumber,
      receiveId: this.selectedReceiveForCredit.id,
      receiveNumber: this.selectedReceiveForCredit.receiveNumber,
      creditDate: new Date(this.creditDateStr).toISOString(),
      reasonType: this.creditReasonType,
      totalCreditAmount: totalClaim,
      availableBalance: totalClaim,
      adjustedAmount: 0,
      status: 'Available',
      notes: this.creditNotes,
      items: this.creditLinesState
    });

    this.isSaving = false;
    this.isCreateCreditModalOpen = false;
    this.toasterService.success(`Credit Note ${newCredit.creditNumber} of ₹${newCredit.totalCreditAmount.toLocaleString('en-IN')} issued successfully!`, 'Credit Note Generated');
    this.loadCredits();

    this.viewCredit(newCredit);
  }

  // Modal: Apply Credit to Bill
  openApplyCreditModal(credit: VendorCreditRecord): void {
    this.selectedCreditForApply = credit;
    const matchingBills = this.getMatchingBillsForCredit(credit);
    if (matchingBills.length > 0) {
      this.targetBillId = matchingBills[0].id;
      this.amountToAdjust = Math.min(credit.availableBalance, matchingBills[0].balanceDue);
    } else {
      this.targetBillId = '';
      this.amountToAdjust = 0;
    }
    this.isApplyCreditModalOpen = true;
  }

  getMatchingBillsForCredit(credit: VendorCreditRecord): PurchaseBillRecord[] {
    return this.openBills.filter(b => b.supplierName.toLowerCase() === credit.supplierName.toLowerCase() && b.balanceDue > 0);
  }

  onTargetBillChanged(): void {
    if (!this.selectedCreditForApply || !this.targetBillId) return;
    const bill = this.openBills.find(b => b.id === this.targetBillId);
    if (bill) {
      this.amountToAdjust = Math.min(this.selectedCreditForApply.availableBalance, bill.balanceDue);
    }
  }

  submitApplyCredit(): void {
    if (!this.selectedCreditForApply || !this.targetBillId) {
      this.toasterService.warn('Please select a purchase bill to adjust this credit note against.');
      return;
    }

    if (this.amountToAdjust <= 0) {
      this.toasterService.warn('Adjustment amount must be greater than zero.');
      return;
    }

    if (this.amountToAdjust > this.selectedCreditForApply.availableBalance) {
      this.toasterService.warn('Adjustment amount cannot exceed available credit balance.');
      return;
    }

    const success = this.creditService.applyCreditToBill(this.selectedCreditForApply.id, this.targetBillId, this.amountToAdjust);
    if (success) {
      this.isApplyCreditModalOpen = false;
      this.toasterService.success(`₹${this.amountToAdjust.toLocaleString('en-IN')} adjusted successfully against bill!`, 'Credit Applied');
      this.loadCredits();
      this.loadBills();
    } else {
      this.toasterService.error('Failed to apply credit note.');
    }
  }

  viewCredit(credit: VendorCreditRecord): void {
    this.selectedCreditForView = credit;
    this.isViewCreditModalOpen = true;
  }

  printCredit(): void {
    window.print();
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Available': return 'wms-badge-success';
      case 'Partially Adjusted': return 'wms-badge-info';
      case 'Fully Adjusted': return 'wms-badge-secondary';
      default: return 'wms-badge-primary';
    }
  }

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

