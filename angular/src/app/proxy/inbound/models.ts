import type { InboundOrderStatus } from './inbound-order-status.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateInboundOrderLineDto {
  productId: string;
  expectedQuantity: number;
  unitPrice?: number;
}

export interface CreateUpdateInboundOrderDto {
  orderNumber: string;
  warehouseId: string;
  supplierName: string;
  supplierInvoiceNumber?: string | null;
  expectedArrivalDate?: string;
  status?: InboundOrderStatus;
  notes?: string | null;
  lines?: CreateInboundOrderLineDto[];
}

export interface GetInboundOrderListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  warehouseId?: string | null;
  status?: InboundOrderStatus | null;
  expectedDateFrom?: string | null;
  expectedDateTo?: string | null;
}

export interface InboundOrderDto extends FullAuditedEntityDto<string> {
  orderNumber?: string;
  warehouseId?: string;
  warehouseCode?: string | null;
  warehouseName?: string | null;
  supplierName?: string;
  supplierInvoiceNumber?: string | null;
  expectedArrivalDate?: string;
  actualArrivalDate?: string | null;
  status?: InboundOrderStatus;
  notes?: string | null;
  totalLines?: number;
  totalExpectedQuantity?: number;
  totalReceivedQuantity?: number;
  lines?: InboundOrderLineDto[];
}

export interface InboundOrderLineDto extends EntityDto<string> {
  inboundOrderId?: string;
  productId?: string;
  productSku?: string | null;
  productName?: string | null;
  expectedQuantity?: number;
  receivedQuantity?: number;
  remainingQuantity?: number;
  unitPrice?: number;
  isCompleted?: boolean;
}

export interface ReceiveInboundGoodsDto {
  inboundOrderId: string;
  inboundOrderLineId: string;
  receivedQuantity: number;
  destinationLocationId: string;
  lotBatchNumber?: string | null;
  serialNumber?: string | null;
  lpn?: string | null;
  expiryDate?: string | null;
  manufactureDate?: string | null;
  notes?: string | null;
}
