import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface SupplierDto extends FullAuditedEntityDto<string> {
  code: string;
  name: string;
  contactPerson?: string;
  email?: string;
  phoneNumber?: string;
  taxNumber?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  paymentTerms?: string;
  isActive: boolean;
  rating?: number;
  notes?: string;
  fullAddress?: string;
}

export interface CreateUpdateSupplierDto {
  code: string;
  name: string;
  contactPerson?: string;
  email?: string;
  phoneNumber?: string;
  taxNumber?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  paymentTerms?: string;
  isActive: boolean;
  rating?: number;
  notes?: string;
}

export interface SupplierLookupDto {
  id: string;
  code: string;
  name: string;
  contactPerson?: string;
  phoneNumber?: string;
  email?: string;
  city?: string;
}

export interface GetSupplierListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  city?: string;
  paymentTerms?: string;
  isActive?: boolean;
}

