import type { ProductCategory } from './product-category.enum';
import type { UnitOfMeasure } from './unit-of-measure.enum';
import type { WarehouseZoneStorageType } from '../warehouses/warehouse-zone-storage-type.enum';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto, EntityDto } from '@abp/ng.core';

export enum ProductType {
  Simple = 1,
  VariantBased = 2,
}

export enum AttributeDataType {
  Dropdown = 1,
  Text = 2,
  Number = 3,
  Boolean = 4,
  MultiSelect = 5,
}

export enum BarcodeSymbology {
  Code128 = 1,
  Ean13 = 2,
  UpcA = 3,
  QrCode = 4,
  DataMatrix = 5,
  Unclassified = 6,
}

export enum BarcodeTargetType {
  ProductVariant = 1,
  LicensePlate = 2,
  WarehouseLocation = 3,
  InventoryLot = 4,
  InventorySerial = 5,
}

export enum BarcodePackagingLevel {
  Each = 1,
  InnerPack = 2,
  Case = 3,
  Pallet = 4,
}

export interface CreateUpdateProductDto {
  sku: string;
  name: string;
  productType?: ProductType;
  manufacturerPartNumber?: string | null;
  barcode?: string | null;
  description?: string | null;
  brand?: string | null;
  imageUrl?: string | null;
  category?: ProductCategory;
  categoryId?: string | null;
  uom?: UnitOfMeasure;
  isLotTracked?: boolean;
  isExpiryTracked?: boolean;
  isSerialTracked?: boolean;
  weightKg?: number;
  volumeCm3?: number;
  lengthCm?: number | null;
  widthCm?: number | null;
  heightCm?: number | null;
  requiredStorageType?: WarehouseZoneStorageType;
  temperatureMinCelsius?: number | null;
  temperatureMaxCelsius?: number | null;
  reorderLevel?: number;
  targetStockLevel?: number;
  unitPrice?: number;
  isActive?: boolean;
}

export interface GetProductListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  category?: ProductCategory | null;
  uom?: UnitOfMeasure | null;
  requiredStorageType?: WarehouseZoneStorageType | null;
  isLotTracked?: boolean | null;
  isExpiryTracked?: boolean | null;
  isSerialTracked?: boolean | null;
  isActive?: boolean | null;
}

export interface ProductDto extends FullAuditedEntityDto<string> {
  sku?: string;
  name?: string;
  productType?: ProductType;
  manufacturerPartNumber?: string | null;
  barcode?: string | null;
  description?: string | null;
  brand?: string | null;
  imageUrl?: string | null;
  category?: ProductCategory;
  categoryId?: string | null;
  categoryName?: string | null;
  categoryCode?: string | null;
  uom?: UnitOfMeasure;
  isLotTracked?: boolean;
  isExpiryTracked?: boolean;
  isSerialTracked?: boolean;
  weightKg?: number;
  volumeCm3?: number;
  lengthCm?: number | null;
  widthCm?: number | null;
  heightCm?: number | null;
  requiredStorageType?: WarehouseZoneStorageType;
  temperatureMinCelsius?: number | null;
  temperatureMaxCelsius?: number | null;
  reorderLevel?: number;
  targetStockLevel?: number;
  unitPrice?: number;
  isActive?: boolean;
  variantCount?: number;
}

export interface ProductVariantDto extends FullAuditedEntityDto<string> {
  productId: string;
  productSku: string;
  productName: string;
  sku: string;
  variantName?: string | null;
  combinationHash: string;
  purchasePrice?: number | null;
  sellingPrice: number;
  weightKg: number;
  volumeCm3: number;
  lengthCm?: number | null;
  widthCm?: number | null;
  heightCm?: number | null;
  imageUrl?: string | null;
  isActive: boolean;
  isDefault: boolean;
  primaryBarcode?: string | null;
  attributes: ProductVariantAttributeValueDto[];
  barcodes: ProductBarcodeDto[];
}

export interface ProductVariantAttributeValueDto {
  attributeId: string;
  attributeName: string;
  attributeCode: string;
  valueId?: string | null;
  valueText: string;
}

export interface ProductBarcodeDto extends EntityDto<string> {
  barcodeValue: string;
  symbology: BarcodeSymbology;
  barcodeKind: number;
  packagingLevel: BarcodePackagingLevel;
  unitsPerPackage: number;
  isPrimary: boolean;
  isActive: boolean;
}

export interface CreateProductVariantDto {
  productId: string;
  sku?: string | null;
  variantName?: string | null;
  purchasePrice?: number | null;
  sellingPrice?: number;
  weightKg?: number;
  volumeCm3?: number;
  lengthCm?: number | null;
  widthCm?: number | null;
  heightCm?: number | null;
  imageUrl?: string | null;
  isDefault?: boolean;
  primaryBarcode?: string | null;
  attributes: VariantAttributeSelectionDto[];
}

export interface VariantAttributeSelectionDto {
  attributeId: string;
  valueId?: string | null;
  valueText: string;
}

export interface UpdateProductVariantDto {
  sku: string;
  variantName?: string | null;
  purchasePrice?: number | null;
  sellingPrice: number;
  weightKg: number;
  volumeCm3: number;
  lengthCm?: number | null;
  widthCm?: number | null;
  heightCm?: number | null;
  isActive: boolean;
  isDefault: boolean;
}

export interface GenerateVariantMatrixInput {
  productId: string;
  options: AttributeMatrixOptionInput[];
}

export interface AttributeMatrixOptionInput {
  attributeId: string;
  attributeName: string;
  attributeCode: string;
  selectedValues: string[];
}

export interface GeneratedVariantPreviewDto {
  generatedSku: string;
  generatedVariantName: string;
  combinationHash: string;
  imageUrl?: string | null;
  attributes: VariantAttributeSelectionDto[];
  sellingPrice?: number;
  purchasePrice?: number;
  weightKg?: number;
}

export interface ProductAttributeDto extends FullAuditedEntityDto<string> {
  name: string;
  code: string;
  dataType: AttributeDataType;
  isVariantDefining: boolean;
  isRequired: boolean;
  displayOrder: number;
  isActive: boolean;
  values: ProductAttributeValueDto[];
}

export interface ProductAttributeValueDto extends EntityDto<string> {
  attributeId: string;
  value: string;
  displayLabel?: string | null;
  colorHex?: string | null;
  displayOrder: number;
  isActive: boolean;
}

export interface CreateUpdateProductAttributeDto {
  name: string;
  code: string;
  dataType: AttributeDataType;
  isVariantDefining: boolean;
  isRequired: boolean;
  displayOrder: number;
  isActive: boolean;
  values: CreateAttributeValueDto[];
}

export interface CreateAttributeValueDto {
  value: string;
  displayLabel?: string | null;
  colorHex?: string | null;
  displayOrder: number;
}

export interface CategoryAttributeTemplateDto extends EntityDto<string> {
  categoryId: string;
  attributeId: string;
  attributeName: string;
  attributeCode: string;
  dataType: AttributeDataType;
  isVariantDefining: boolean;
  isRequired: boolean;
  displayOrder: number;
  predefinedValues: ProductAttributeValueDto[];
}

export interface AssignCategoryAttributeDto {
  categoryId: string;
  attributeId: string;
  isVariantDefining: boolean;
  isRequired: boolean;
  displayOrder: number;
}

export interface CategoryDto extends FullAuditedEntityDto<string> {
  code: string;
  name: string;
  description?: string | null;
  parentCategoryId?: string | null;
  parentCategoryName?: string | null;
  parentCategoryCode?: string | null;
  subCategoriesCount: number;
  isActive: boolean;
}

export interface CreateUpdateCategoryDto {
  code: string;
  name: string;
  description?: string | null;
  parentCategoryId?: string | null;
  isActive: boolean;
}

export interface GetCategoryListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  parentCategoryId?: string | null;
  isRootOnly?: boolean | null;
  isActive?: boolean | null;
}

export interface CategoryLookupDto {
  id: string;
  code: string;
  name: string;
  parentCategoryId?: string | null;
  parentCategoryName?: string | null;
  displayName: string;
}
