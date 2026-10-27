import { mapEnumToOptions } from '@abp/ng.core';

export enum UnitOfMeasure {
  Pieces = 1,
  Boxes = 2,
  Cartons = 3,
  Pallets = 4,
  Kilograms = 5,
  Grams = 6,
  Liters = 7,
  Meters = 8,
}

export const unitOfMeasureOptions = mapEnumToOptions(UnitOfMeasure);
