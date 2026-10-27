import { mapEnumToOptions } from '@abp/ng.core';

export enum ProductCategory {
  Pharmaceuticals = 1,
  Electronics = 2,
  Apparel = 3,
  FoodAndBeverage = 4,
  Chemicals = 5,
  FastMovingConsumerGoods = 6,
  AutomotiveParts = 7,
  General = 8,
}

export const productCategoryOptions = mapEnumToOptions(ProductCategory);
