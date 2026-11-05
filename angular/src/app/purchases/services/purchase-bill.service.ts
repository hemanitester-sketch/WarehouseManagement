import { Injectable, inject } from '@angular/core';
import { PurchaseReceiveService, PurchaseReceiveRecord } from './purchase-receive.service';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

export interface PurchaseBillItemRecord {
  productId: string;
  productSku: string;
  productName: string;
  productVariantId?: string | null;
  variantSku?: string | null;
  variantName?: string | null;
  hsnCode?: string;
  quantity: number; // Accepted quantity
  unitPrice: number;
  discountPercent: number;
  discountAmount: number;
  taxRatePercent: number; // 0, 5, 12, 18, 28%
  taxAmount: number;
  lineTotal: number;
}

export interface PurchaseBillRecord {
  id: string;
  billNumber: string; // e.g. BILL-2026-0001
  vendorInvoiceNumber: string;
  inboundOrderId?: string;
  orderNumber?: string;
  receiveId?: string;
  receiveNumber?: string; // GRN Number
  supplierId?: string;
  supplierName: string;
  supplierGstin?: string;
  supplierAddress?: string;
  warehouseId: string;
  warehouseName: string;
  warehouseCode?: string;
  billDate: string; // ISO
  dueDate: string; // ISO
  paymentTerms: string; // Net 30, Net 15, Advance, Immediate
  status: 'Draft' | 'Open' | 'Partially Paid' | 'Paid' | 'Overdue';
  subTotal: number;
  totalDiscount: number;
  freightCharges: number;
  totalTax: number;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  roundOff: number;
  grandTotal: number;
  paidAmount: number;
  balanceDue: number;
  notes?: string;
  items: PurchaseBillItemRecord[];
}

const BILLS_STORAGE_KEY = 'wms_purchase_bills_v1';

@Injectable({
  providedIn: 'root'
})
export class PurchaseBillService {
  private receiveService = inject(PurchaseReceiveService);

  private getStoredBills(): PurchaseBillRecord[] {
    try {
      const data = localStorage.getItem(BILLS_STORAGE_KEY);
      return data ? JSON.parse(data) : [];
    } catch {
      return [];
    }
  }

  private saveStoredBills(records: PurchaseBillRecord[]): void {
    try {
      localStorage.setItem(BILLS_STORAGE_KEY, JSON.stringify(records));
    } catch (e) {
      console.error('Failed to save bills to local storage', e);
    }
  }

  generateBillNumber(): string {
    const year = new Date().getFullYear();
    const stored = this.getStoredBills();
    const nextSeq = stored.length + 1;
    const padded = nextSeq.toString().padStart(4, '0');
    return `BILL-${year}-${padded}`;
  }

  getBills(): Observable<PurchaseBillRecord[]> {
    const stored = this.getStoredBills();

    // Auto-synthesize initial bill if stored is empty and we have receives
    return this.receiveService.getReceives().pipe(
      map(receives => {
        const existingGrns = new Set(stored.map(b => b.receiveNumber).filter(Boolean));
        let addedCount = 0;

        receives.forEach(rec => {
          if (!existingGrns.has(rec.receiveNumber) && rec.totalAcceptedQuantity > 0) {
            const year = new Date().getFullYear();
            const seq = (stored.length + addedCount + 1).toString().padStart(4, '0');
            const recDate = new Date(rec.receivedDate);
            const dueDate = new Date(recDate.getTime() + 30 * 24 * 60 * 60 * 1000);

            // Compute line items
            const items: PurchaseBillItemRecord[] = (rec.items || []).map(item => {
              const qty = item.quantityAccepted || item.quantityReceived || 1;
              const rate = item.unitPrice > 0 ? item.unitPrice : 150;
              const disc = 0;
              const taxable = (qty * rate) - disc;
              const taxRate = 18; // default 18% GST
              const tax = Math.round((taxable * taxRate / 100) * 100) / 100;
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
                discountPercent: 0,
                discountAmount: disc,
                taxRatePercent: taxRate,
                taxAmount: tax,
                lineTotal: taxable + tax
              };
            });

            const subTotal = items.reduce((sum, i) => sum + (i.quantity * i.unitPrice), 0);
            const totalTax = items.reduce((sum, i) => sum + i.taxAmount, 0);
            const freight = 250;
            const grandTotal = Math.round(subTotal + totalTax + freight);

            const synthesized: PurchaseBillRecord = {
              id: 'bill-' + rec.id,
              billNumber: `BILL-${year}-${seq}`,
              vendorInvoiceNumber: rec.supplierInvoiceNumber || `INV-${seq}`,
              inboundOrderId: rec.inboundOrderId,
              orderNumber: rec.orderNumber,
              receiveId: rec.id,
              receiveNumber: rec.receiveNumber,
              supplierName: rec.supplierName,
              supplierGstin: '07AABCA5678Q1Z2',
              supplierAddress: 'Plot 42, Industrial Area, Sector 62',
              warehouseId: rec.warehouseId,
              warehouseName: rec.warehouseName,
              warehouseCode: rec.warehouseCode,
              billDate: rec.receivedDate,
              dueDate: dueDate.toISOString(),
              paymentTerms: 'Net 30',
              status: 'Open',
              subTotal: subTotal,
              totalDiscount: 0,
              freightCharges: freight,
              totalTax: totalTax,
              cgstAmount: Math.round(totalTax / 2 * 100) / 100,
              sgstAmount: Math.round(totalTax / 2 * 100) / 100,
              igstAmount: 0,
              roundOff: 0,
              grandTotal: grandTotal,
              paidAmount: 0,
              balanceDue: grandTotal,
              notes: `Generated from Goods Receipt Note ${rec.receiveNumber}`,
              items: items
            };
            stored.unshift(synthesized);
            addedCount++;
          }
        });

        if (addedCount > 0) {
          this.saveStoredBills(stored);
        }

        return stored.sort((a, b) => new Date(b.billDate).getTime() - new Date(a.billDate).getTime());
      })
    );
  }

  getBillById(id: string): Observable<PurchaseBillRecord | undefined> {
    const list = this.getStoredBills();
    return of(list.find(b => b.id === id || b.billNumber === id));
  }

  recordBill(record: Omit<PurchaseBillRecord, 'id' | 'billNumber'>): PurchaseBillRecord {
    const stored = this.getStoredBills();
    const newRecord: PurchaseBillRecord = {
      ...record,
      id: 'bill-' + Date.now() + '-' + Math.floor(Math.random() * 1000),
      billNumber: this.generateBillNumber()
    };
    stored.unshift(newRecord);
    this.saveStoredBills(stored);
    return newRecord;
  }

  updateBillStatus(id: string, status: PurchaseBillRecord['status'], paidAmount: number): void {
    const stored = this.getStoredBills();
    const found = stored.find(b => b.id === id);
    if (found) {
      found.status = status;
      found.paidAmount = paidAmount;
      found.balanceDue = Math.max(0, found.grandTotal - paidAmount);
      this.saveStoredBills(stored);
    }
  }
}

