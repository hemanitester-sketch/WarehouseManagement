import { Injectable, inject } from '@angular/core';
import { PurchaseReceiveService, PurchaseReceiveRecord } from './purchase-receive.service';
import { PurchaseBillService, PurchaseBillRecord } from './purchase-bill.service';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

export interface VendorCreditItemRecord {
  productId: string;
  productSku: string;
  productName: string;
  quantityDamagedOrReturned: number;
  unitPrice: number;
  taxRatePercent: number;
  taxAmount: number;
  claimAmount: number;
  reason: string;
}

export interface VendorCreditRecord {
  id: string;
  creditNumber: string; // e.g. VCR-2026-0001
  supplierName: string;
  supplierGstin?: string;
  orderNumber?: string;
  receiveId?: string;
  receiveNumber?: string; // GRN Number
  billId?: string;
  billNumber?: string;
  creditDate: string; // ISO
  reasonType: 'Damaged in Transit' | 'Defective / Rejected at QC' | 'Short Shipment' | 'Price Variance' | 'Return to Vendor (RTV)';
  totalCreditAmount: number;
  availableBalance: number;
  adjustedAmount: number;
  status: 'Available' | 'Partially Adjusted' | 'Fully Adjusted' | 'Refunded';
  notes?: string;
  adjustedBillNumber?: string;
  items: VendorCreditItemRecord[];
}

const CREDITS_STORAGE_KEY = 'wms_vendor_credits_v1';

@Injectable({
  providedIn: 'root'
})
export class VendorCreditService {
  private receiveService = inject(PurchaseReceiveService);
  private billService = inject(PurchaseBillService);

  private getStoredCredits(): VendorCreditRecord[] {
    try {
      const data = localStorage.getItem(CREDITS_STORAGE_KEY);
      return data ? JSON.parse(data) : [];
    } catch {
      return [];
    }
  }

  private saveStoredCredits(records: VendorCreditRecord[]): void {
    try {
      localStorage.setItem(CREDITS_STORAGE_KEY, JSON.stringify(records));
    } catch (e) {
      console.error('Failed to save vendor credits to local storage', e);
    }
  }

  generateCreditNumber(): string {
    const year = new Date().getFullYear();
    const stored = this.getStoredCredits();
    const nextSeq = stored.length + 1;
    const padded = nextSeq.toString().padStart(4, '0');
    return `VCR-${year}-${padded}`;
  }

  getCredits(): Observable<VendorCreditRecord[]> {
    const stored = this.getStoredCredits();

    // Auto-synthesize initial vendor credit if stored is empty and we have receives with damaged goods
    return this.receiveService.getReceives().pipe(
      map(receives => {
        if (stored.length === 0) {
          const damagedRec = receives.find(r => (r.totalDamagedQuantity || 0) > 0);
          const year = new Date().getFullYear();

          if (damagedRec) {
            const damagedItems = (damagedRec.items || []).filter(i => (i.quantityDamaged || 0) > 0);
            const items: VendorCreditItemRecord[] = damagedItems.map(item => {
              const qty = item.quantityDamaged || 1;
              const rate = item.unitPrice > 0 ? item.unitPrice : 120;
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

            const total = items.reduce((sum, i) => sum + i.claimAmount, 0) || 566;

            const initialCredit: VendorCreditRecord = {
              id: 'vcr-init-1',
              creditNumber: `VCR-${year}-0001`,
              supplierName: damagedRec.supplierName,
              supplierGstin: '07AABCA5678Q1Z2',
              orderNumber: damagedRec.orderNumber,
              receiveId: damagedRec.id,
              receiveNumber: damagedRec.receiveNumber,
              creditDate: new Date().toISOString(),
              reasonType: 'Damaged in Transit',
              totalCreditAmount: total,
              availableBalance: total,
              adjustedAmount: 0,
              status: 'Available',
              notes: `Credit note issued for ${damagedRec.totalDamagedQuantity} damaged unit(s) reported in GRN ${damagedRec.receiveNumber}.`,
              items: items
            };
            stored.push(initialCredit);
            this.saveStoredCredits(stored);
          } else if (receives.length > 0) {
            // Create a demonstration claim credit note
            const firstRec = receives[0];
            const initialCredit: VendorCreditRecord = {
              id: 'vcr-demo-1',
              creditNumber: `VCR-${year}-0001`,
              supplierName: firstRec.supplierName,
              supplierGstin: '07AABCA5678Q1Z2',
              orderNumber: firstRec.orderNumber,
              receiveId: firstRec.id,
              receiveNumber: firstRec.receiveNumber,
              creditDate: new Date().toISOString(),
              reasonType: 'Damaged in Transit',
              totalCreditAmount: 708,
              availableBalance: 708,
              adjustedAmount: 0,
              status: 'Available',
              notes: 'Credit note issued for water-damaged carton reported at dock inward receiving.',
              items: [{
                productId: 'demo-prod-1',
                productSku: 'SKU-MED-001',
                productName: 'Damaged Items Batch',
                quantityDamagedOrReturned: 2,
                unitPrice: 300,
                taxRatePercent: 18,
                taxAmount: 108,
                claimAmount: 708,
                reason: 'Water damaged outer box and broken tamper seal'
              }]
            };
            stored.push(initialCredit);
            this.saveStoredCredits(stored);
          }
        }

        return stored.sort((a, b) => new Date(b.creditDate).getTime() - new Date(a.creditDate).getTime());
      })
    );
  }

  recordCredit(record: Omit<VendorCreditRecord, 'id' | 'creditNumber'>): VendorCreditRecord {
    const stored = this.getStoredCredits();
    const newRecord: VendorCreditRecord = {
      ...record,
      id: 'vcr-' + Date.now() + '-' + Math.floor(Math.random() * 1000),
      creditNumber: this.generateCreditNumber()
    };
    stored.unshift(newRecord);
    this.saveStoredCredits(stored);
    return newRecord;
  }

  applyCreditToBill(creditId: string, billId: string, amountToApply: number): boolean {
    const stored = this.getStoredCredits();
    const credit = stored.find(c => c.id === creditId);
    if (!credit || credit.availableBalance < amountToApply) return false;

    // Deduct from credit note
    credit.availableBalance = Math.max(0, credit.availableBalance - amountToApply);
    credit.adjustedAmount = (credit.adjustedAmount || 0) + amountToApply;
    credit.status = credit.availableBalance <= 0 ? 'Fully Adjusted' : 'Partially Adjusted';

    // Update bill
    this.billService.getBills().subscribe(bills => {
      const bill = bills.find(b => b.id === billId);
      if (bill) {
        credit.adjustedBillNumber = bill.billNumber;
        const newPaid = (bill.paidAmount || 0) + amountToApply;
        const newStatus = (bill.grandTotal - newPaid) <= 0 ? 'Paid' : 'Partially Paid';
        this.billService.updateBillStatus(bill.id, newStatus, newPaid);
      }
      this.saveStoredCredits(stored);
    });

    return true;
  }
}

