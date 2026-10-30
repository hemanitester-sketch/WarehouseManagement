import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export enum CycleCountType {
  ABCCategory = 1,
  ZoneLocationSweep = 2,
  ProductSpecific = 3,
  RandomSampling = 4,
  FullWallToWall = 5,
}

export enum CycleCountStatus {
  Draft = 1,
  InProgress = 2,
  UnderReview = 3,
  ReCountRequested = 4,
  Reconciled = 5,
  Cancelled = 6,
}

export enum DiscrepancyReason {
  None = 0,
  ShrinkageOrTheft = 1,
  MisplacedInWrongBin = 2,
  DamagedOrExpired = 3,
  DataEntryMismatch = 4,
  InboundShortage = 5,
  PickingError = 6,
  Other = 7,
}

export interface CycleCountAuditItemDto {
  id: string;
  cycleCountAuditId: string;
  productId: string;
  productName: string;
  productSku: string;
  productBarcode?: string;
  categoryName?: string;
  locationId: string;
  locationCode: string;
  zoneName?: string;
  lotBatchNumber?: string;
  serialNumber?: string;
  systemQuantity: number;
  countedQuantity?: number;
  varianceQuantity: number;
  unitCost: number;
  varianceValue: number;
  discrepancyReason: DiscrepancyReason;
  reasonNotes?: string;
  isReCounted: boolean;
  countedAt?: string;
}

export interface CycleCountAuditDto extends FullAuditedEntityDto<string> {
  auditNumber: string;
  warehouseId: string;
  warehouseName: string;
  warehouseCode: string;
  zoneId?: string;
  zoneName?: string;
  countType: CycleCountType;
  status: CycleCountStatus;
  isBlindCount: boolean;
  scheduledDate: string;
  completedDate?: string;
  assignedToFacultyId?: string;
  assignedAuditorName?: string;
  approvedByFacultyName?: string;
  approvedDate?: string;
  totalVarianceQuantity: number;
  totalVarianceValue: number;
  totalItemsCount: number;
  countedItemsCount: number;
  discrepancyItemsCount: number;
  accuracyPercentage: number;
  notes?: string;
  items: CycleCountAuditItemDto[];
}

export interface CreateCycleCountAuditItemDto {
  productId: string;
  locationId: string;
  lotBatchNumber?: string;
  serialNumber?: string;
  systemQuantity: number;
  unitCost: number;
}

export interface CreateCycleCountAuditDto {
  warehouseId: string;
  zoneId?: string;
  specificProductId?: string;
  countType: CycleCountType;
  isBlindCount: boolean;
  scheduledDate: string;
  assignedToFacultyId?: string;
  assignedAuditorName?: string;
  notes?: string;
  items: CreateCycleCountAuditItemDto[];
}

export interface RecordCountItemDto {
  itemId: string;
  countedQuantity: number;
  discrepancyReason: DiscrepancyReason;
  reasonNotes?: string;
}

export interface SubmitCountDto {
  notes?: string;
}

export interface ApproveReconcileAuditDto {
  approverName?: string;
  approvalNotes?: string;
}

export interface GetCycleCountListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  warehouseId?: string;
  zoneId?: string;
  countType?: CycleCountType;
  status?: CycleCountStatus;
  fromDate?: string;
  toDate?: string;
}

export interface CycleCountDashboardMetricsDto {
  totalAudits: number;
  inProgressCounts: number;
  auditsUnderReview: number;
  reconciledAudits: number;
  overallAccuracyRate: number;
  totalNetVarianceUnits: number;
  totalNetVarianceValue: number;
}

