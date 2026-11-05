import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { PurchasePaymentService, PaymentTransactionRecord } from '../services/purchase-payment.service';
import { PurchaseBillService, PurchaseBillRecord } from '../services/purchase-bill.service';

@Component({
  selector: 'app-payments-mode',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './payments-mode.component.html',
  styleUrls: ['./payments-mode.component.scss']
})
export class PaymentsModeComponent implements OnInit {
  private paymentService = inject(PurchasePaymentService);
  private billService = inject(PurchaseBillService);
  private toasterService = inject(ToasterService);
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);

  payments: PaymentTransactionRecord[] = [];
  openBills: PurchaseBillRecord[] = [];

  isLoading = false;
  isSaving = false;
  searchQuery = '';
  selectedModeFilter = '';

  // Pagination
  currentPage = 1;
  pageSize = 50;
  Math = Math;

  // Modals
  isRecordPaymentModalOpen = false;
  isViewVoucherModalOpen = false;
  selectedPaymentForView: PaymentTransactionRecord | null = null;

  // Payment Form State
  selectedBillId: string = '';
  selectedBillForPayment: PurchaseBillRecord | null = null;
  paymentMode: PaymentTransactionRecord['paymentMode'] = 'Bank Transfer (NEFT/RTGS)';
  paymentReferenceNumber = '';
  paidThroughAccount = 'HDFC Bank - Current A/c (0042)';
  paymentDateStr = new Date().toISOString().split('T')[0];
  paymentAmount: number = 0;
  paymentNotes = 'Disbursed against approved vendor bill as per payment terms.';
  authorizedByName = 'Finance Controller';

  configuredAccounts = [
    'HDFC Bank - Current A/c (0042)',
    'ICICI Bank - Operations A/c (9124)',
    'State Bank of India - Commercial A/c (5510)',
    'Corporate UPI / Virtual Payment Gateway',
    'Main Warehouse Petty Cash Ledger'
  ];

  ngOnInit(): void {
    this.loadBills();
    this.loadPayments();

    // Check query params if redirected from Bills page
    this.route.queryParams.subscribe(params => {
      if (params['billId']) {
        setTimeout(() => {
          this.openRecordPaymentModal(params['billId']);
        }, 300);
      }
    });
  }

  loadBills(): void {
    this.billService.getBills().subscribe({
      next: (list) => {
        const all = list || [];
        this.openBills = all.filter(b => (b.balanceDue || 0) > 0);
        this.cdr.detectChanges();
      }
    });
  }

  loadPayments(): void {
    this.isLoading = true;
    this.paymentService.getPayments().subscribe({
      next: (list) => {
        this.payments = list || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Filtered Payments
  get filteredPayments(): PaymentTransactionRecord[] {
    let list = this.payments;

    if (this.selectedModeFilter) {
      list = list.filter(p => p.paymentMode === this.selectedModeFilter);
    }

    if (this.searchQuery && this.searchQuery.trim()) {
      const q = this.searchQuery.trim().toLowerCase();
      list = list.filter(p => 
        (p.paymentNumber && p.paymentNumber.toLowerCase().includes(q)) ||
        (p.billNumber && p.billNumber.toLowerCase().includes(q)) ||
        (p.vendorInvoiceNumber && p.vendorInvoiceNumber.toLowerCase().includes(q)) ||
        (p.orderNumber && p.orderNumber.toLowerCase().includes(q)) ||
        (p.referenceNumber && p.referenceNumber.toLowerCase().includes(q)) ||
        (p.supplierName && p.supplierName.toLowerCase().includes(q)) ||
        (p.paidThroughAccount && p.paidThroughAccount.toLowerCase().includes(q))
      );
    }

    return list;
  }

  get paginatedPayments(): PaymentTransactionRecord[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredPayments.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredPayments.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    if (this.filteredPayments.length === 0) return 0;
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredPayments.length);
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
  get totalPaymentsCount(): number {
    return this.payments.length;
  }

  get totalAmountDisbursed(): number {
    return this.payments.reduce((sum, p) => sum + (p.amountPaid || 0), 0);
  }

  get totalOutstandingPayable(): number {
    return this.openBills.reduce((sum, b) => sum + (b.balanceDue || 0), 0);
  }

  // Modal: Open Record Payment
  openRecordPaymentModal(preselectedBillId?: string): void {
    this.paymentDateStr = new Date().toISOString().split('T')[0];
    this.paymentMode = 'Bank Transfer (NEFT/RTGS)';
    this.paymentReferenceNumber = `UTR${Math.floor(1000000000 + Math.random() * 9000000000)}`;
    this.paidThroughAccount = 'HDFC Bank - Current A/c (0042)';
    this.paymentNotes = 'Disbursed against approved vendor bill as per payment terms.';
    this.authorizedByName = 'Finance Controller';

    if (preselectedBillId) {
      this.selectedBillId = preselectedBillId;
    } else if (this.openBills.length > 0) {
      this.selectedBillId = this.openBills[0].id;
    } else {
      this.selectedBillId = '';
    }

    this.onBillSelected();
    this.isRecordPaymentModalOpen = true;
  }

  onBillSelected(): void {
    if (!this.selectedBillId) {
      this.selectedBillForPayment = null;
      this.paymentAmount = 0;
      return;
    }

    this.selectedBillForPayment = this.openBills.find(b => b.id === this.selectedBillId || b.billNumber === this.selectedBillId) || null;
    if (this.selectedBillForPayment) {
      this.paymentAmount = this.selectedBillForPayment.balanceDue || this.selectedBillForPayment.grandTotal;
    }
  }

  setQuickAmount(percentage: number): void {
    if (!this.selectedBillForPayment) return;
    const balance = this.selectedBillForPayment.balanceDue || this.selectedBillForPayment.grandTotal;
    this.paymentAmount = Math.round(balance * (percentage / 100));
  }

  get remainingBalanceAfterPayment(): number {
    if (!this.selectedBillForPayment) return 0;
    const currentBalance = this.selectedBillForPayment.balanceDue;
    return Math.max(0, currentBalance - (this.paymentAmount || 0));
  }

  submitPayment(): void {
    if (!this.selectedBillForPayment) {
      this.toasterService.warn('Please select a purchase bill to pay.');
      return;
    }

    if (this.paymentAmount <= 0) {
      this.toasterService.warn('Payment amount must be greater than zero.');
      return;
    }

    if (!this.paymentReferenceNumber.trim()) {
      this.toasterService.warn('Please enter a reference / UTR / transaction number.');
      return;
    }

    this.isSaving = true;

    const remainingBal = this.remainingBalanceAfterPayment;

    const createdPayment = this.paymentService.recordPayment({
      billId: this.selectedBillForPayment.id,
      billNumber: this.selectedBillForPayment.billNumber,
      vendorInvoiceNumber: this.selectedBillForPayment.vendorInvoiceNumber,
      orderNumber: this.selectedBillForPayment.orderNumber,
      receiveNumber: this.selectedBillForPayment.receiveNumber,
      supplierName: this.selectedBillForPayment.supplierName,
      paymentDate: new Date(this.paymentDateStr).toISOString(),
      paymentMode: this.paymentMode,
      referenceNumber: this.paymentReferenceNumber.trim(),
      paidThroughAccount: this.paidThroughAccount,
      amountPaid: this.paymentAmount,
      billTotalAmount: this.selectedBillForPayment.grandTotal,
      remainingBalanceDue: remainingBal,
      status: 'Completed',
      notes: this.paymentNotes,
      authorizedBy: this.authorizedByName
    });

    this.isSaving = false;
    this.isRecordPaymentModalOpen = false;
    this.toasterService.success(`Payment ${createdPayment.paymentNumber} of ₹${createdPayment.amountPaid.toLocaleString('en-IN')} recorded successfully!`, 'Disbursement Confirmed');

    this.loadBills();
    this.loadPayments();

    // View voucher immediately
    this.viewVoucher(createdPayment);
  }

  viewVoucher(payment: PaymentTransactionRecord): void {
    this.selectedPaymentForView = payment;
    this.isViewVoucherModalOpen = true;
  }

  printVoucher(): void {
    window.print();
  }

  getPaymentModeIcon(mode: string): string {
    if (mode.includes('UPI')) return 'bi bi-qr-code';
    if (mode.includes('Cheque')) return 'bi bi-card-text';
    if (mode.includes('Cash')) return 'bi bi-cash';
    if (mode.includes('Card')) return 'bi bi-credit-card-2-front';
    return 'bi bi-bank';
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

