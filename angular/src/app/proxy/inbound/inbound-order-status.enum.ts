import { mapEnumToOptions } from '@abp/ng.core';

export enum InboundOrderStatus {
  Draft = 1,
  InTransit = 2,
  Arrived = 3,
  PartiallyReceived = 4,
  Completed = 5,
  Cancelled = 6,
}

export const inboundOrderStatusOptions = mapEnumToOptions(InboundOrderStatus);
