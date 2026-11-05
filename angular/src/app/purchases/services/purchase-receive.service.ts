import { Injectable, inject } from '@angular/core';
import { InboundOrderService } from '../../proxy/inbound/inbound-order.service';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

export interface PurchaseReceiveItemRecord {
  productId: string;
  productSku: string;
  productName: string;
  productVariantId?: string | null;
  variantSku?: string | null;
  variantName?: string | null;
  quantityOrdered: number;
  quantityReceived: number;
  quantityAccepted: number;
  quantityDamaged: number;
  rejectionReason?: string;
  destinationLocationId: string;
  destinationLocationCode: string;
  lotBatchNumber?: string;
  serialNumber?: string;
  expiryDate?: string;
  unitPrice: number;
}

export interface PurchaseReceiveRecord {
  id: string;
  receiveNumber: string; // e.g. GRN-2026-0001
  inboundOrderId: string;
  orderNumber: string; // e.g. PO-2026-1049
  supplierName: string;
  warehouseId: string;
  warehouseName: string;
  warehouseCode?: string;
  supplierInvoiceNumber?: string;
  carrierName?: string;
  vehicleNumber?: string;
  gatePassNumber?: string;
  receivedDate: string; // ISO string
  status: 'Completed' | 'Partially Received' | 'Under Inspection';
  totalOrderedQuantity: number;
  totalReceivedQuantity: number;
  totalAcceptedQuantity: number;
  totalDamagedQuantity: number;
  notes?: string;
  inspectedBy?: string;
  items: PurchaseReceiveItemRecord[];
}

const STORAGE_KEY = 'wms_purchase_receives_v1';

@Injectable({
  providedIn: 'root'
})
export class PurchaseReceiveService {
  private inboundService = inject(InboundOrderService);

  private getStoredReceives(): PurchaseReceiveRecord[] {
    try {
      const data = localStorage.getItem(STORAGE_KEY);
      return data ? JSON.parse(data) : [];
    } catch {
      return [];
    }
  }

  private saveStoredReceives(records: PurchaseReceiveRecord[]): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(records));
    } catch (e) {
      console.error('Failed to save purchase receives to local storage', e);
    }
  }

  generateReceiveNumber(): string {
    const year = new Date().getFullYear();
    const stored = this.getStoredReceives();
    const nextSeq = stored.length + 1;
    const padded = nextSeq.toString().padStart(4, '0');
    return `GRN-${year}-${padded}`;
  }

  getReceives(): Observable<PurchaseReceiveRecord[]> {
    const stored = this.getStoredReceives();

    // Auto-synthesize from existing POs if stored is empty or missing old POs
    return this.inboundService.getList({ maxResultCount: 100, skipCount: 0 }).pipe(
      map(res => {
        const orders = res.items || [];
        const existingOrderNumbers = new Set(stored.map(s => s.orderNumber));
        let addedCount = 0;

        orders.forEach((order) => {
          const recQty = order.totalReceivedQuantity || 0;
          const expQty = order.totalExpectedQuantity || 0;
          const poNum = order.orderNumber || 'PO-ORDER';

          if (recQty > 0 && !existingOrderNumbers.has(poNum)) {
            const year = new Date().getFullYear();
            const seq = (stored.length + addedCount + 1).toString().padStart(4, '0');
            const arrDate = order.actualArrivalDate || order.creationTime;
            const recDateStr = arrDate ? new Date(arrDate).toISOString() : new Date().toISOString();

            const synthesized: PurchaseReceiveRecord = {
              id: 'rec-' + (order.id || Date.now()),
              receiveNumber: `GRN-${year}-${seq}`,
              inboundOrderId: order.id || '',
              orderNumber: poNum,
              supplierName: order.supplierName || 'Supplier',
              warehouseId: order.warehouseId || '',
              warehouseName: order.warehouseName || 'Main Hub',
              warehouseCode: order.warehouseCode || 'WH',
              supplierInvoiceNumber: order.supplierInvoiceNumber || 'INV-REC',
              carrierName: 'Direct Dispatch / Logistics',
              vehicleNumber: 'DL-01-TRK-2026',
              gatePassNumber: `GP-${seq}`,
              receivedDate: recDateStr,
              status: recQty >= expQty ? 'Completed' : 'Partially Received',
              totalOrderedQuantity: expQty,
              totalReceivedQuantity: recQty,
              totalAcceptedQuantity: recQty,
              totalDamagedQuantity: 0,
              notes: order.notes || 'Goods received and inspected at dock',
              inspectedBy: 'Warehouse Dock Supervisor',
              items: (order.lines || []).filter(l => (l.receivedQuantity || 0) > 0).map(l => ({
                productId: l.productId || '',
                productSku: l.productSku || 'SKU',
                productName: l.productName || 'Product Item',
                productVariantId: l.productVariantId,
                variantSku: l.variantSku,
                variantName: l.variantName,
                quantityOrdered: l.expectedQuantity || 0,
                quantityReceived: l.receivedQuantity || 0,
                quantityAccepted: l.receivedQuantity || 0,
                quantityDamaged: 0,
                destinationLocationId: '',
                destinationLocationCode: 'RECEIVING-DOCK',
                lotBatchNumber: 'LOT-2026-REC',
                unitPrice: l.unitPrice || 0
              }))
            };
            stored.unshift(synthesized);
            addedCount++;
          }
        });

        if (addedCount > 0) {
          this.saveStoredReceives(stored);
        }

        // Return sorted by receivedDate descending
        return stored.sort((a, b) => new Date(b.receivedDate).getTime() - new Date(a.receivedDate).getTime());
      })
    );
  }

  getReceiveById(id: string): Observable<PurchaseReceiveRecord | undefined> {
    const list = this.getStoredReceives();
    return of(list.find(r => r.id === id || r.receiveNumber === id));
  }

  recordReceive(record: Omit<PurchaseReceiveRecord, 'id' | 'receiveNumber'>): PurchaseReceiveRecord {
    const stored = this.getStoredReceives();
    const newRecord: PurchaseReceiveRecord = {
      ...record,
      id: 'rec-' + Date.now() + '-' + Math.floor(Math.random() * 1000),
      receiveNumber: this.generateReceiveNumber()
    };
    stored.unshift(newRecord);
    this.saveStoredReceives(stored);
    return newRecord;
  }
}
