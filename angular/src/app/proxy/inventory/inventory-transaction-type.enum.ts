import { mapEnumToOptions } from '@abp/ng.core';

export enum InventoryTransactionType {
  InboundReceipt = 1,
  OutboundPick = 2,
  BinTransfer = 3,
  AdjustmentIncrease = 4,
  AdjustmentDecrease = 5,
  DamageWriteOff = 6,
  QuarantineRelease = 7,
}

export const inventoryTransactionTypeOptions = mapEnumToOptions(InventoryTransactionType);
