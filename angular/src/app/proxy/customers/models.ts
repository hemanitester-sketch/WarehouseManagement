import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export enum CustomerType {
  CorporateB2B = 1,
  RetailB2C = 2,
  Distributor = 3,
  EcommercePartner = 4,
  Government = 5
}

export enum CustomerStatus {
  Active = 1,
  UnderReview = 2,
  OnHold = 3,
  Inactive = 4
}

export interface CustomerDto extends FullAuditedEntityDto<string> {
  code: string;
  name: string;
  customerType: CustomerType;
  customerTypeName: string;
  industry?: string;
  contactPerson?: string;
  email?: string;
  phoneNumber?: string;
  alternativePhone?: string;
  taxNumber?: string;
  panNumber?: string;
  billingAddress?: string;
  shippingAddress?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  paymentTerms?: string;
  creditLimit: number;
  currentOutstanding: number;
  creditUtilizationPercent: number;
  priceTier?: string;
  deliveryDockHours?: string;
  specialInstructions?: string;
  status: CustomerStatus;
  statusName: string;
  isActive: boolean;
  rating?: number;
  totalOrdersPlaced: number;
  totalRevenue: number;
  notes?: string;
}

export interface CreateUpdateCustomerDto {
  code: string;
  name: string;
  customerType: CustomerType;
  industry?: string;
  contactPerson?: string;
  email?: string;
  phoneNumber?: string;
  alternativePhone?: string;
  taxNumber?: string;
  panNumber?: string;
  billingAddress?: string;
  shippingAddress?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  paymentTerms?: string;
  creditLimit: number;
  currentOutstanding: number;
  priceTier?: string;
  deliveryDockHours?: string;
  specialInstructions?: string;
  status: CustomerStatus;
  isActive: boolean;
  rating?: number;
  notes?: string;
}

export interface GetCustomerListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  customerType?: CustomerType;
  status?: CustomerStatus;
  city?: string;
  paymentTerms?: string;
  isActive?: boolean;
}

export interface CustomerLookupDto {
  id: string;
  code: string;
  name: string;
  customerType: CustomerType;
  contactPerson?: string;
  phoneNumber?: string;
  email?: string;
  shippingAddress?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  paymentTerms?: string;
  creditLimit: number;
  currentOutstanding: number;
}

export interface CustomerDashboardMetricsDto {
  totalClientsCount: number;
  activeClientsCount: number;
  corporateB2BCount: number;
  retailDistributorCount: number;
  totalCreditLimitExposure: number;
  totalCurrentOutstanding: number;
  averageCreditUtilizationPercent: number;
  onHoldClientsCount: number;
}

