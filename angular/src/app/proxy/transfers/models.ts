import type { PagedAndSortedResultRequestDto } from '@abp/ng.core';

export enum TransferType {
  InterWarehouse = 0,
  BinToBin = 1,
  Replenishment = 2,
  Quarantine = 3,
}

export const transferTypeOptions = [
  { value: TransferType.InterWarehouse, label: 'Inter-Warehouse' },
  { value: TransferType.BinToBin, label: 'Internal Bin-to-Bin' },
  { value: TransferType.Replenishment, label: 'Replenishment' },
  { value: TransferType.Quarantine, label: 'Quarantine / Hold' },
];

export enum TransferStatus {
  Draft = 0,
  Approved = 1,
  InTransit = 2,
  Completed = 3,
  Cancelled = 4,
}

export const transferStatusOptions = [
  { value: TransferStatus.Draft, label: 'Draft' },
  { value: TransferStatus.Approved, label: 'Approved' },
  { value: TransferStatus.InTransit, label: 'In Transit' },
  { value: TransferStatus.Completed, label: 'Completed' },
  { value: TransferStatus.Cancelled, label: 'Cancelled' },
];

export interface StockTransferItemDto {
  id: string;
  stockTransferId: string;
  productId: string;
  productName?: string;
  productSku?: string;
  sourceLocationId?: string;
  sourceLocationCode?: string;
  destinationLocationId?: string;
  destinationLocationCode?: string;
  requestedQuantity: number;
  transferredQuantity: number;
  unitCost?: number;
  notes?: string;
}

export interface StockTransferDto {
  id: string;
  transferNumber: string;
  type: TransferType;
  status: TransferStatus;
  sourceWarehouseId: string;
  sourceWarehouseName?: string;
  destinationWarehouseId: string;
  destinationWarehouseName?: string;
  carrierName?: string;
  trackingNumber?: string;
  vehicleNumber?: string;
  driverName?: string;
  driverPhone?: string;
  notes?: string;
  transferDate: string;
  dispatchedDate?: string;
  completedDate?: string;
  deliveryChallanNumber?: string;
  totalItemsCount: number;
  totalUnits: number;
  items: StockTransferItemDto[];
}

export interface CreateStockTransferItemDto {
  productId: string;
  sourceLocationId?: string;
  destinationLocationId?: string;
  requestedQuantity: number;
  quantityRequested?: number;
  unitCost?: number;
  notes?: string;
}

export interface CreateUpdateStockTransferDto {
  transferNumber?: string;
  type: TransferType;
  transferType?: TransferType;
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  sourceLocationId?: string;
  destinationLocationId?: string;
  carrierName?: string;
  trackingNumber?: string;
  vehicleNumber?: string;
  driverName?: string;
  driverPhone?: string;
  notes?: string;
  transferDate: string;
  deliveryChallanNumber?: string;
  items: CreateStockTransferItemDto[];
}

export interface DispatchTransferDto {
  carrierName?: string;
  trackingNumber?: string;
  vehicleNumber?: string;
  driverName?: string;
  driverPhone?: string;
  deliveryChallanNumber?: string;
}

export interface GetStockTransferListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  type?: TransferType;
  status?: TransferStatus;
  sourceWarehouseId?: string;
  destinationWarehouseId?: string;
}

