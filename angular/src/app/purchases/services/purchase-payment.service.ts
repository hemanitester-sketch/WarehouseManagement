import { Injectable, inject } from '@angular/core';
import { PurchaseBillService, PurchaseBillRecord } from './purchase-bill.service';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

export interface PaymentTransactionRecord {
  id: string;
  paymentNumber: string; // e.g. PAY-2026-0001
  billId: string;
  billNumber: string;
  vendorInvoiceNumber: string;
  orderNumber?: string;
  receiveNumber?: string;
  supplierName: string;
  paymentDate: string; // ISO
  paymentMode: 'Bank Transfer (NEFT/RTGS)' | 'UPI Transfer' | 'Company Cheque' | 'Cash Ledger' | 'Corporate Card';
  referenceNumber: string; // UTR, Cheque #, Transaction ID
  paidThroughAccount: string; // e.g. HDFC Current A/c, ICICI Bank, Cash
  amountPaid: number;
  billTotalAmount: number;
  remainingBalanceDue: number;
  status: 'Completed' | 'Pending Clearance';
  notes?: string;
  authorizedBy?: string;
}

const PAYMENTS_STORAGE_KEY = 'wms_purchase_payments_v1';

@Injectable({
  providedIn: 'root'
})
export class PurchasePaymentService {
  private billService = inject(PurchaseBillService);

  private getStoredPayments(): PaymentTransactionRecord[] {
    try {
      const data = localStorage.getItem(PAYMENTS_STORAGE_KEY);
      return data ? JSON.parse(data) : [];
    } catch {
      return [];
    }
  }

  private saveStoredPayments(records: PaymentTransactionRecord[]): void {
    try {
      localStorage.setItem(PAYMENTS_STORAGE_KEY, JSON.stringify(records));
    } catch (e) {
      console.error('Failed to save payments to local storage', e);
    }
  }

  generatePaymentNumber(): string {
    const year = new Date().getFullYear();
    const stored = this.getStoredPayments();
    const nextSeq = stored.length + 1;
    const padded = nextSeq.toString().padStart(4, '0');
    return `PAY-${year}-${padded}`;
  }

  getPayments(): Observable<PaymentTransactionRecord[]> {
    const stored = this.getStoredPayments();

    // Auto-synthesize initial payment record if stored is empty and we have bills
    return this.billService.getBills().pipe(
      map(bills => {
        if (stored.length === 0 && bills.length > 0) {
          const firstBill = bills[0];
          const year = new Date().getFullYear();
          const paidAmt = Math.round(firstBill.grandTotal * 0.5); // 50% advance / partial payment
          const remaining = firstBill.grandTotal - paidAmt;

          const initialPayment: PaymentTransactionRecord = {
            id: 'pay-init-1',
            paymentNumber: `PAY-${year}-0001`,
            billId: firstBill.id,
            billNumber: firstBill.billNumber,
            vendorInvoiceNumber: firstBill.vendorInvoiceNumber,
            orderNumber: firstBill.orderNumber,
            receiveNumber: firstBill.receiveNumber,
            supplierName: firstBill.supplierName,
            paymentDate: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
            paymentMode: 'Bank Transfer (NEFT/RTGS)',
            referenceNumber: 'UTR9821740192',
            paidThroughAccount: 'HDFC Bank - Current A/c (0042)',
            amountPaid: paidAmt,
            billTotalAmount: firstBill.grandTotal,
            remainingBalanceDue: remaining,
            status: 'Completed',
            notes: 'Advance 50% payment disbursed via NEFT against purchase order receipt.',
            authorizedBy: 'Finance Controller'
          };
          stored.push(initialPayment);
          this.saveStoredPayments(stored);

          // Update bill status
          this.billService.updateBillStatus(firstBill.id, 'Partially Paid', paidAmt);
        }

        return stored.sort((a, b) => new Date(b.paymentDate).getTime() - new Date(a.paymentDate).getTime());
      })
    );
  }

  recordPayment(record: Omit<PaymentTransactionRecord, 'id' | 'paymentNumber'>): PaymentTransactionRecord {
    const stored = this.getStoredPayments();
    const newRecord: PaymentTransactionRecord = {
      ...record,
      id: 'pay-' + Date.now() + '-' + Math.floor(Math.random() * 1000),
      paymentNumber: this.generatePaymentNumber()
    };
    stored.unshift(newRecord);
    this.saveStoredPayments(stored);

    // Update the linked bill status and balances
    const newPaidTotal = (record.billTotalAmount - record.remainingBalanceDue);
    const newStatus = record.remainingBalanceDue <= 0 ? 'Paid' : 'Partially Paid';
    this.billService.updateBillStatus(record.billId, newStatus, newPaidTotal);

    return newRecord;
  }
}

