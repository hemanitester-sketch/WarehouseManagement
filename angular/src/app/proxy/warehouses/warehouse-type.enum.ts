import { mapEnumToOptions } from '@abp/ng.core';

export enum WarehouseType {
  DistributionCenter = 1,
  FulfillmentCenter = 2,
  ColdStorage = 3,
  RegionalWarehouse = 4,
  PlantWarehouse = 5,
  RetailBackroom = 6,
}

export const warehouseTypeOptions = mapEnumToOptions(WarehouseType);
