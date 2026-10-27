import type { OrderPriority } from './order-priority.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { OutboundOrderStatus } from './outbound-order-status.enum';

export interface AllocateOrderStockDto {
  outboundOrderId: string;
}

export interface CreateOutboundOrderLineDto {
  productId: string;
  orderedQuantity: number;
  unitPrice?: number;
}

export interface CreateUpdateOutboundOrderDto {
  orderNumber: string;
  warehouseId: string;
  customerName: string;
  shippingAddress?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
  priority?: OrderPriority;
  orderDate?: string;
  requiredDeliveryDate?: string | null;
  notes?: string | null;
  lines?: CreateOutboundOrderLineDto[];
}

export interface GetOutboundOrderListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  warehouseId?: string | null;
  status?: OutboundOrderStatus | null;
  priority?: OrderPriority | null;
  orderDateFrom?: string | null;
  orderDateTo?: string | null;
}

export interface OutboundOrderDto extends FullAuditedEntityDto<string> {
  orderNumber?: string;
  warehouseId?: string;
  warehouseCode?: string | null;
  warehouseName?: string | null;
  customerName?: string;
  shippingAddress?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
  priority?: OrderPriority;
  status?: OutboundOrderStatus;
  orderDate?: string;
  requiredDeliveryDate?: string | null;
  shippedDate?: string | null;
  carrierName?: string | null;
  trackingNumber?: string | null;
  notes?: string | null;
  totalLines?: number;
  totalOrderedQuantity?: number;
  totalAllocatedQuantity?: number;
  totalPickedQuantity?: number;
  lines?: OutboundOrderLineDto[];
}

export interface OutboundOrderLineDto extends EntityDto<string> {
  outboundOrderId?: string;
  productId?: string;
  productSku?: string | null;
  productName?: string | null;
  orderedQuantity?: number;
  allocatedQuantity?: number;
  pickedQuantity?: number;
  remainingToPickQuantity?: number;
  unitPrice?: number;
  isCompleted?: boolean;
}

export interface PickOrderGoodsDto {
  outboundOrderId: string;
  outboundOrderLineId: string;
  sourceInventoryItemId: string;
  pickedQuantity: number;
  notes?: string | null;
}

export interface ShipOrderDto {
  outboundOrderId: string;
  carrierName?: string | null;
  trackingNumber?: string | null;
}
