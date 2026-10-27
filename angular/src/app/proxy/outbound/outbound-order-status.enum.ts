import { mapEnumToOptions } from '@abp/ng.core';

export enum OutboundOrderStatus {
  Draft = 1,
  Allocated = 2,
  Picking = 3,
  Picked = 4,
  Packed = 5,
  Shipped = 6,
  Cancelled = 7,
}

export const outboundOrderStatusOptions = mapEnumToOptions(OutboundOrderStatus);
