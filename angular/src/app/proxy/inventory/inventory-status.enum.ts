import { mapEnumToOptions } from '@abp/ng.core';

export enum InventoryStatus {
  Available = 1,
  Quarantined = 2,
  Damaged = 3,
  Expired = 4,
  InTransit = 5,
}

export const inventoryStatusOptions = mapEnumToOptions(InventoryStatus);
