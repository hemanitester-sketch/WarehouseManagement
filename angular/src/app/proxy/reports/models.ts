export interface GetWarehouseReportFilterDto {
  warehouseId?: string;
  fromDate?: string;
  toDate?: string;
  filter?: string;
}

export interface ExecutiveWarehouseKpisDto {
  totalInventoryValuation: number;
  totalStockUnits: number;
  totalActiveSkus: number;
  lowStockSkuCount: number;
  overallSpaceUtilizationRate: number;
  totalOccupiedBins: number;
  totalWarehouseLocations: number;
  inboundThroughput30Days: number;
  outboundThroughput30Days: number;
  auditAccuracyRate: number;
  returnRatePercentage: number;
  pendingInboundOrders: number;
  pendingOutboundOrders: number;
  totalSalvageOrScrapValue: number;
}

export interface StockAgingReportItemDto {
  productId: string;
  sku: string;
  productName: string;
  categoryName: string;
  warehouseId: string;
  warehouseName: string;
  quantityOnHand: number;
  unitPrice: number;
  totalValuation: number;
  oldestReceivedDate?: string;
  ageInDays: number;
  ageBracket: string;
  fsnClassification: string;
  outboundShippedLast90Days: number;
}

export interface ZoneUtilizationSummaryDto {
  zoneId: string;
  zoneName: string;
  storageType: string;
  totalBins: number;
  occupiedBins: number;
  utilizationPercentage: number;
}

export interface SpaceUtilizationReportItemDto {
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  city?: string;
  totalZones: number;
  totalBins: number;
  occupiedBins: number;
  availableBins: number;
  utilizationPercentage: number;
  totalStockUnitsStored: number;
  zoneBreakdown: ZoneUtilizationSummaryDto[];
}

export interface InboundSupplierSlaReportItemDto {
  supplierId?: string;
  supplierCode: string;
  supplierName: string;
  totalPurchaseOrders: number;
  completedOrders: number;
  pendingOrders: number;
  totalExpectedQuantity: number;
  totalReceivedQuantity: number;
  fulfillmentRatePercentage: number;
  onTimeDeliveries: number;
  onTimeDeliveryPercentage: number;
}

export interface CustomerFulfillmentSummaryDto {
  customerId?: string;
  customerCode: string;
  customerName: string;
  totalOrders: number;
  totalUnitsDelivered: number;
  totalOrderValue: number;
}

export interface OutboundFulfillmentReportItemDto {
  totalOutboundOrders: number;
  shippedOrders: number;
  pendingOrders: number;
  cancelledOrders: number;
  totalUnitsShipped: number;
  fulfillmentRatePercentage: number;
  onTimeDispatchPercentage: number;
  topCustomers: CustomerFulfillmentSummaryDto[];
}

export interface ReasonBreakdownDto {
  reason: string;
  count: number;
  percentage: number;
}

export interface QualityReverseLogisticsReportDto {
  totalAuditsConducted: number;
  totalItemsAudited: number;
  netVarianceUnits: number;
  netVarianceCostValue: number;
  auditAccuracyRatePercentage: number;
  totalRmaReturns: number;
  totalReturnedUnits: number;
  totalRefundValue: number;
  totalRestockedUnits: number;
  totalScrappedUnits: number;
  restockRecoveryRatePercentage: number;
  topReturnReasons: ReasonBreakdownDto[];
}

