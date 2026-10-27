import { mapEnumToOptions } from '@abp/ng.core';

export enum OrderPriority {
  Normal = 1,
  High = 2,
  Urgent = 3,
}

export const orderPriorityOptions = mapEnumToOptions(OrderPriority);
