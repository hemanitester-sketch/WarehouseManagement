import type { ProductCategory } from './product-category.enum';
import type { UnitOfMeasure } from './unit-of-measure.enum';
import type { WarehouseZoneStorageType } from '../warehouses/warehouse-zone-storage-type.enum';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateProductDto {
  sku: string;
  name: string;
  barcode?: string | null;
  description?: string | null;
  brand?: string | null;
  category?: ProductCategory;
  uom?: UnitOfMeasure;
  isLotTracked?: boolean;
  isExpiryTracked?: boolean;
  isSerialTracked?: boolean;
  weightKg?: number;
  volumeCm3?: number;
  lengthCm?: number | null;
  widthCm?: number | null;
  heightCm?: number | null;
  requiredStorageType?: WarehouseZoneStorageType;
  temperatureMinCelsius?: number | null;
  temperatureMaxCelsius?: number | null;
  reorderLevel?: number;
  targetStockLevel?: number;
  unitPrice?: number;
  isActive?: boolean;
}

export interface GetProductListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  category?: ProductCategory | null;
  uom?: UnitOfMeasure | null;
  requiredStorageType?: WarehouseZoneStorageType | null;
  isLotTracked?: boolean | null;
  isExpiryTracked?: boolean | null;
  isSerialTracked?: boolean | null;
  isActive?: boolean | null;
}

export interface ProductDto extends FullAuditedEntityDto<string> {
  sku?: string;
  name?: string;
  barcode?: string | null;
  description?: string | null;
  brand?: string | null;
  category?: ProductCategory;
  uom?: UnitOfMeasure;
  isLotTracked?: boolean;
  isExpiryTracked?: boolean;
  isSerialTracked?: boolean;
  weightKg?: number;
  volumeCm3?: number;
  lengthCm?: number | null;
  widthCm?: number | null;
  heightCm?: number | null;
  requiredStorageType?: WarehouseZoneStorageType;
  temperatureMinCelsius?: number | null;
  temperatureMaxCelsius?: number | null;
  reorderLevel?: number;
  targetStockLevel?: number;
  unitPrice?: number;
  isActive?: boolean;
}
