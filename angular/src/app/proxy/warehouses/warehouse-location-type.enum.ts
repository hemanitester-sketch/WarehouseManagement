import { mapEnumToOptions } from '@abp/ng.core';

export enum WarehouseLocationType {
  PalletRack = 1,
  Shelf = 2,
  FlowRack = 3,
  FloorStorage = 4,
  Cantilever = 5,
  StagingLane = 6,
  DockDoor = 7,
}

export const warehouseLocationTypeOptions = mapEnumToOptions(WarehouseLocationType);
