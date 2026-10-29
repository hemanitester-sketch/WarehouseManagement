import type { WarehouseType } from './warehouse-type.enum';
import type { WarehouseLocationType } from './warehouse-location-type.enum';
import type { WarehouseLocationStatus } from './warehouse-location-status.enum';
import type { WarehouseZoneStorageType } from './warehouse-zone-storage-type.enum';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateWarehouseDto {
  code: string;
  name: string;
  type: WarehouseType;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
  managerId?: string | null;
  contactPerson?: string | null;
  contactPhone?: string | null;
  contactEmail?: string | null;
  totalAreaSqFt?: number;
  isActive?: boolean;
}

export interface CreateUpdateWarehouseLocationDto {
  warehouseId: string;
  zoneId: string;
  barcode: string;
  aisle: string;
  rack: string;
  shelf: string;
  bin: string;
  locationType?: WarehouseLocationType;
  status?: WarehouseLocationStatus;
  maxWeightKg?: number;
  maxVolumeCm3?: number;
  lengthCm?: number | null;
  widthCm?: number | null;
  heightCm?: number | null;
  isPickFace?: boolean;
  isActive?: boolean;
}

export interface CreateUpdateWarehouseZoneDto {
  warehouseId: string;
  code: string;
  name: string;
  storageType?: WarehouseZoneStorageType;
  temperatureMinCelsius?: number | null;
  temperatureMaxCelsius?: number | null;
  isActive?: boolean;
}

export interface GetWarehouseListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  type?: WarehouseType | null;
  isActive?: boolean | null;
}

export interface GetWarehouseLocationListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  warehouseId?: string | null;
  zoneId?: string | null;
  locationType?: WarehouseLocationType | null;
  status?: WarehouseLocationStatus | null;
  isPickFace?: boolean | null;
  isActive?: boolean | null;
}

export interface GetWarehouseZoneListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  warehouseId?: string | null;
  storageType?: WarehouseZoneStorageType | null;
  isActive?: boolean | null;
}

export interface WarehouseDto extends FullAuditedEntityDto<string> {
  code?: string;
  name?: string;
  type?: WarehouseType;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
  managerId?: string | null;
  managerName?: string | null;
  managerDesignation?: string | null;
  contactPerson?: string | null;
  contactPhone?: string | null;
  contactEmail?: string | null;
  totalAreaSqFt?: number;
  isActive?: boolean;
}

export interface WarehouseLocationDto extends FullAuditedEntityDto<string> {
  warehouseId?: string;
  zoneId?: string;
  barcode?: string;
  aisle?: string;
  rack?: string;
  shelf?: string;
  bin?: string;
  locationType?: WarehouseLocationType;
  status?: WarehouseLocationStatus;
  maxWeightKg?: number;
  maxVolumeCm3?: number;
  lengthCm?: number | null;
  widthCm?: number | null;
  heightCm?: number | null;
  isPickFace?: boolean;
  isActive?: boolean;
}

export interface WarehouseZoneDto extends FullAuditedEntityDto<string> {
  warehouseId?: string;
  code?: string;
  name?: string;
  storageType?: WarehouseZoneStorageType;
  temperatureMinCelsius?: number | null;
  temperatureMaxCelsius?: number | null;
  isActive?: boolean;
}
