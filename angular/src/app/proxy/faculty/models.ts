import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface FacultyMemberDto extends FullAuditedEntityDto<string> {
  employeeCode: string;
  fullName: string;
  designation: string;
  department?: string | null;
  email?: string | null;
  phoneNumber?: string | null;
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  warehouseId?: string | null;
  warehouseName?: string | null;
  warehouseCode?: string | null;
  assignedWarehouseId?: string | null;
  assignedWarehouseName?: string | null;
  assignedWarehouseCode?: string | null;
  shift?: string | null;
  joiningDate?: string | null;
  qualifications?: string | null;
  status?: string | null;
  notes?: string | null;
  isActive: boolean;
}

export interface CreateUpdateFacultyMemberDto {
  employeeCode: string;
  fullName: string;
  designation: string;
  department?: string | null;
  email?: string | null;
  phoneNumber?: string | null;
  emergencyContactName?: string | null;
  emergencyContactPhone?: string | null;
  warehouseId?: string | null;
  assignedWarehouseId?: string | null;
  shift?: string | null;
  joiningDate?: string | null;
  qualifications?: string | null;
  status?: string | null;
  notes?: string | null;
  isActive?: boolean;
}

export interface FacultyMemberLookupDto {
  id: string;
  employeeCode: string;
  fullName: string;
  designation: string;
  phoneNumber?: string | null;
  email?: string | null;
  displayName: string;
}

export interface GetFacultyMemberListDto extends PagedAndSortedResultRequestDto {
  filter?: string | null;
  warehouseId?: string | null;
  assignedWarehouseId?: string | null;
  designation?: string | null;
  shift?: string | null;
  status?: string | null;
  isActive?: boolean | null;
}
