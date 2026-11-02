import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export enum ReturnOrderStatus {
  Draft = 1,
  Requested = 2,
  Approved = 3,
  InTransit = 4,
  Received = 5,
  UnderInspection = 6,
  Completed = 7,
  Rejected = 8,
  Cancelled = 9,
}

export enum ReturnType {
  CustomerReturn = 1,
  VendorReturn = 2,
  InterBranchReturn = 3,
}

export enum ReturnReason {
  DefectiveItem = 1,
  WrongItemShipped = 2,
  DamagedInTransit = 3,
  CustomerCancellation = 4,
  QualityDiscrepancy = 5,
  ExpiredProduct = 6,
  ExcessStock = 7,
  Other = 8,
}

export enum ItemConditionGrade {
  GradeA_BrandNew = 1,
  GradeB_OpenBox = 2,
  GradeC_Defective = 3,
  GradeD_Scrap = 4,
}

export enum ItemResolutionAction {
  PendingInspection = 1,
  RestockToInventory = 2,
  ScrapAndWriteOff = 3,
  ReturnToVendor = 4,
  QuarantineHold = 5,
}

export interface ReturnOrderItemDto extends FullAuditedEntityDto<string> {
  returnOrderId: string;
  productId: string;
  productSku: string;
  productName: string;
  expectedQuantity: number;
  receivedQuantity: number;
  restockedQuantity: number;
  scrappedQuantity: number;
  unitRefundPrice: number;
  returnReason: ReturnReason;
  returnReasonName?: string;
  conditionGrade?: ItemConditionGrade;
  conditionGradeName?: string;
  resolutionAction: ItemResolutionAction;
  resolutionActionName?: string;
  destinationLocationId?: string;
  destinationLocationBarcode?: string;
  lotBatchNumber?: string;
  serialNumber?: string;
  inspectionNotes?: string;
}

export interface ReturnOrderDto extends FullAuditedEntityDto<string> {
  rmaNumber: string;
  returnType: ReturnType;
  returnTypeName?: string;
  status: ReturnOrderStatus;
  statusName?: string;
  warehouseId: string;
  warehouseCode?: string;
  warehouseName?: string;
  customerId?: string;
  customerName?: string;
  supplierId?: string;
  supplierName?: string;
  outboundOrderId?: string;
  outboundOrderNumber?: string;
  inboundOrderId?: string;
  inboundOrderNumber?: string;
  carrierName?: string;
  trackingNumber?: string;
  returnDate: string;
  receivedDate?: string;
  inspectedDate?: string;
  inspectedBy?: string;
  totalQuantityExpected: number;
  totalQuantityReceived: number;
  totalQuantityRestocked: number;
  totalQuantityScrapped: number;
  totalRefundOrCreditAmount: number;
  notes?: string;
  items: ReturnOrderItemDto[];
}

export interface CreateReturnOrderItemDto {
  productId: string;
  expectedQuantity: number;
  unitRefundPrice?: number;
  returnReason: ReturnReason;
  lotBatchNumber?: string;
  serialNumber?: string;
}

export interface CreateReturnOrderDto {
  returnType: ReturnType;
  warehouseId: string;
  customerId?: string;
  customerName?: string;
  supplierId?: string;
  supplierName?: string;
  outboundOrderId?: string;
  outboundOrderNumber?: string;
  inboundOrderId?: string;
  inboundOrderNumber?: string;
  carrierName?: string;
  trackingNumber?: string;
  returnDate?: string;
  notes?: string;
  items: CreateReturnOrderItemDto[];
}

export interface GetReturnOrderListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  warehouseId?: string;
  customerId?: string;
  supplierId?: string;
  returnType?: ReturnType;
  status?: ReturnOrderStatus;
  fromDate?: string;
  toDate?: string;
}

export interface ReceiveReturnItemDto {
  itemId: string;
  receivedQuantity: number;
}

export interface ReceiveReturnDto {
  receivedDate?: string;
  notes?: string;
  items: ReceiveReturnItemDto[];
}

export interface InspectItemResolutionDto {
  itemId: string;
  conditionGrade: ItemConditionGrade;
  resolutionAction: ItemResolutionAction;
  restockedQuantity: number;
  scrappedQuantity: number;
  destinationLocationId?: string;
  inspectionNotes?: string;
}

export interface InspectReturnOrderDto {
  inspectorName?: string;
  items: InspectItemResolutionDto[];
}

export interface ReturnDashboardMetricsDto {
  totalReturns: number;
  pendingApprovalCount: number;
  inTransitCount: number;
  awaitingInspectionCount: number;
  completedCount: number;
  totalRestockedUnits: number;
  totalScrappedUnits: number;
  totalRefundValue: number;
  recoveryRatePercentage: number;
}
