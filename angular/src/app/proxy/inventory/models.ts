import type { InventoryStatus } from './inventory-status.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { InventoryTransactionType } from './inventory-transaction-type.enum';

export interface AdjustStockDto {
  warehouseId: string;
  locationId: string;
  productId: string;
  quantityDelta: number;
  lotBatchNumber?: string | null;
  serialNumber?: string | null;
  lpn?: string | null;
  expiryDate?: string | null;
  manufactureDate?: string | null;
  status?: InventoryStatus;
  reason: string;
  notes?: string | null;
}

export interface GetInventoryListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  warehouseId?: string | null;
  zoneId?: string | null;
  locationId?: string | null;
  productId?: string | null;
  lotBatchNumber?: string | null;
  serialNumber?: string | null;
  lpn?: string | null;
  status?: InventoryStatus | null;
  expiredOnly?: boolean | null;
}

export interface GetInventoryTransactionListDto extends PagedAndSortedResultRequestDto {
  warehouseId?: string | null;
  productId?: string | null;
  transactionType?: InventoryTransactionType | null;
  referenceNumber?: string | null;
}

export interface InventoryItemDto extends FullAuditedEntityDto<string> {
  warehouseId?: string;
  warehouseCode?: string | null;
  warehouseName?: string | null;
  zoneId?: string;
  zoneCode?: string | null;
  locationId?: string;
  locationBarcode?: string | null;
  locationCoordinates?: string | null;
  productId?: string;
  productSku?: string | null;
  productName?: string | null;
  quantityOnHand?: number;
  quantityAllocated?: number;
  quantityAvailable?: number;
  lotBatchNumber?: string | null;
  serialNumber?: string | null;
  lpn?: string | null;
  expiryDate?: string | null;
  manufactureDate?: string | null;
  status?: InventoryStatus;
  receivedDate?: string;
}

export interface InventorySummaryDto {
  productId?: string;
  productSku?: string;
  productName?: string;
  warehouseId?: string;
  warehouseCode?: string;
  totalOnHand?: number;
  totalAllocated?: number;
  totalAvailable?: number;
  totalQuarantined?: number;
  totalDamaged?: number;
  locationCount?: number;
}

export interface InventoryTransactionDto extends EntityDto<string> {
  warehouseId?: string;
  productId?: string;
  fromLocationId?: string | null;
  toLocationId?: string | null;
  quantity?: number;
  lotBatchNumber?: string | null;
  serialNumber?: string | null;
  lpn?: string | null;
  transactionType?: InventoryTransactionType;
  referenceNumber?: string | null;
  notes?: string | null;
  transactionTime?: string;
}

export interface TransferStockDto {
  sourceInventoryItemId: string;
  destinationWarehouseId: string;
  destinationLocationId: string;
  quantity: number;
  referenceNumber?: string | null;
  notes?: string | null;
}
