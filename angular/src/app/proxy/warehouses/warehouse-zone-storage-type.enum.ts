import { mapEnumToOptions } from '@abp/ng.core';

export enum WarehouseZoneStorageType {
  Ambient = 1,
  ColdStorage = 2,
  Frozen = 3,
  Hazardous = 4,
  HighValue = 5,
  BulkReserve = 6,
  PickFace = 7,
  StagingInbound = 8,
  StagingOutbound = 9,
}

export const warehouseZoneStorageTypeOptions = mapEnumToOptions(WarehouseZoneStorageType);
