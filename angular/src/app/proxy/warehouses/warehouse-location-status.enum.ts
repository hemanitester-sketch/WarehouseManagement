import { mapEnumToOptions } from '@abp/ng.core';

export enum WarehouseLocationStatus {
  Available = 1,
  Occupied = 2,
  Blocked = 3,
  UnderMaintenance = 4,
}

export const warehouseLocationStatusOptions = mapEnumToOptions(WarehouseLocationStatus);
