import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { FacultyMemberService } from '../proxy/faculty/faculty-member.service';
import { FacultyMemberDto, CreateUpdateFacultyMemberDto } from '../proxy/faculty/models';
import { WarehouseService } from '../proxy/warehouses/warehouse.service';
import { WarehouseDto } from '../proxy/warehouses/models';

@Component({
  selector: 'app-faculty',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './faculty.component.html',
  styleUrls: ['./faculty.component.scss'],
})
export class FacultyComponent implements OnInit {
  private facultyService = inject(FacultyMemberService);
  private warehouseService = inject(WarehouseService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  facultyMembers: FacultyMemberDto[] = [];
  filteredMembers: FacultyMemberDto[] = [];
  warehouses: WarehouseDto[] = [];

  isLoading = false;
  isSaving = false;

  // Filters
  searchQuery = '';
  selectedWarehouseFilter = 'all';
  selectedShiftFilter = 'all';
  selectedStatusFilter = 'all';
  selectedDesignationFilter = 'all';

  // Pagination State (50 limit)
  currentPage = 1;
  pageSize = 50;

  get paginatedMembers(): FacultyMemberDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredMembers.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredMembers.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    return this.filteredMembers.length === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredMembers.length);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  prevPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize = Number(newSize);
    this.currentPage = 1;
  }

  // Metrics
  totalFaculty = 0;
  activeManagers = 0;
  assignedFacilities = 0;
  onLeaveOrInactive = 0;

  // Drawer / Modal State
  isModalOpen = false;
  isEditing = false;
  editingId: string | null = null;
  modalTitle = 'Register New Faculty & Manager';

  // Common Designations Preset
  designationPresets: string[] = [
    'Warehouse Operations Manager',
    'Facility General Manager',
    'Assistant Warehouse Manager',
    'Inventory Control Supervisor',
    'Logistics & Dispatch Lead',
    'Quality & Safety Officer',
    'Shift Incharge / Supervisor',
    'Dock & Receiving Supervisor',
  ];

  // Common Shifts Preset
  shiftPresets: string[] = [
    'General (09:00 AM - 06:00 PM)',
    'Morning (06:00 AM - 02:30 PM)',
    'Evening (02:00 PM - 10:30 PM)',
    'Night (10:00 PM - 06:30 AM)',
    'Rotational Roster',
  ];

  // Departments Preset
  departmentPresets: string[] = [
    'Warehouse Operations',
    'Inventory Management',
    'Logistics & Fulfillment',
    'Health, Safety & Compliance',
    'Administration & Security',
    'Cold Chain & Perishables',
  ];

  formModel: CreateUpdateFacultyMemberDto = {
    employeeCode: '',
    fullName: '',
    designation: 'Warehouse Operations Manager',
    department: 'Warehouse Operations',
    email: '',
    phoneNumber: '',
    emergencyContactName: '',
    emergencyContactPhone: '',
    warehouseId: null,
    shift: 'General (09:00 AM - 06:00 PM)',
    joiningDate: new Date().toISOString().substring(0, 10),
    qualifications: '',
    status: 'Active',
    notes: '',
    isActive: true,
  };

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadFacultyMembers();
  }

  loadWarehouses(): void {
    this.warehouseService.getList({ maxResultCount: 200, isActive: true }).subscribe({
      next: (res) => {
        this.warehouses = res.items || [];
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading warehouses for faculty module:', err);
      },
    });
  }

  loadFacultyMembers(): void {
    this.isLoading = true;
    this.facultyService.getList({ maxResultCount: 300, sorting: 'EmployeeCode asc' }).subscribe({
      next: (res) => {
        this.facultyMembers = (res.items || []).map((m: any) => ({
          ...m,
          warehouseId: m.assignedWarehouseId || m.warehouseId || null,
          warehouseName: m.assignedWarehouseName || m.warehouseName || null,
          warehouseCode: m.assignedWarehouseCode || m.warehouseCode || null,
          status: m.isActive ? 'Active' : 'Inactive',
        }));
        this.updateMetrics();
        this.applyFilters();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading faculty members:', err);
        this.toasterService.error('Failed to load faculty & facility managers list.', 'Error');
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  updateMetrics(): void {
    this.totalFaculty = this.facultyMembers.length;
    this.activeManagers = this.facultyMembers.filter(
      (m) => m.isActive && (m.designation?.toLowerCase().includes('manager') || m.designation?.toLowerCase().includes('lead'))
    ).length;
    const warehouseIdSet = new Set(this.facultyMembers.filter((m) => !!m.warehouseId).map((m) => m.warehouseId));
    this.assignedFacilities = warehouseIdSet.size;
    this.onLeaveOrInactive = this.facultyMembers.filter(
      (m) => !m.isActive || m.status === 'On Leave' || m.status === 'Inactive'
    ).length;
  }

  applyFilters(): void {
    const query = (this.searchQuery || '').trim().toLowerCase();

    this.filteredMembers = this.facultyMembers.filter((m) => {
      // 1. Search Query
      const matchesSearch =
        !query ||
        m.employeeCode.toLowerCase().includes(query) ||
        m.fullName.toLowerCase().includes(query) ||
        (m.designation && m.designation.toLowerCase().includes(query)) ||
        (m.department && m.department.toLowerCase().includes(query)) ||
        (m.email && m.email.toLowerCase().includes(query)) ||
        (m.phoneNumber && m.phoneNumber.toLowerCase().includes(query)) ||
        (m.warehouseName && m.warehouseName.toLowerCase().includes(query));

      // 2. Warehouse Filter
      const matchesWarehouse =
        this.selectedWarehouseFilter === 'all' ||
        (this.selectedWarehouseFilter === 'unassigned' && !m.warehouseId) ||
        m.warehouseId === this.selectedWarehouseFilter;

      // 3. Shift Filter
      const matchesShift =
        this.selectedShiftFilter === 'all' ||
        (m.shift && m.shift.toLowerCase().includes(this.selectedShiftFilter.toLowerCase()));

      // 4. Status Filter
      const matchesStatus =
        this.selectedStatusFilter === 'all' ||
        (this.selectedStatusFilter === 'active' && m.isActive) ||
        (this.selectedStatusFilter === 'inactive' && !m.isActive);

      // 5. Designation Filter
      const matchesDesignation =
        this.selectedDesignationFilter === 'all' ||
        (m.designation && m.designation.toLowerCase().includes(this.selectedDesignationFilter.toLowerCase()));

      return matchesSearch && matchesWarehouse && matchesShift && matchesStatus && matchesDesignation;
    });
    this.currentPage = 1;
  }

  resetFilters(): void {
    this.searchQuery = '';
    this.selectedWarehouseFilter = 'all';
    this.selectedShiftFilter = 'all';
    this.selectedStatusFilter = 'all';
    this.selectedDesignationFilter = 'all';
    this.applyFilters();
  }

  // Drawer / Modal Actions
  openCreateModal(): void {
    this.isEditing = false;
    this.editingId = null;
    this.modalTitle = 'Register New Faculty & Manager';

    // Auto-generate next employee code like FAC-101
    const nextCode = this.generateNextEmployeeCode();

    this.formModel = {
      employeeCode: nextCode,
      fullName: '',
      designation: 'Warehouse Operations Manager',
      department: 'Warehouse Operations',
      email: '',
      phoneNumber: '',
      emergencyContactName: '',
      emergencyContactPhone: '',
      warehouseId: this.warehouses.length > 0 ? (this.warehouses[0].id || null) : null,
      shift: 'General (09:00 AM - 06:00 PM)',
      joiningDate: new Date().toISOString().substring(0, 10),
      qualifications: '',
      status: 'Active',
      notes: '',
      isActive: true,
    };

    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  openEditModal(member: FacultyMemberDto): void {
    this.isEditing = true;
    this.editingId = member.id || null;
    this.modalTitle = `Edit Faculty: ${member.fullName}`;

    this.formModel = {
      employeeCode: member.employeeCode,
      fullName: member.fullName,
      designation: member.designation,
      department: member.department || '',
      email: member.email || '',
      phoneNumber: member.phoneNumber || '',
      emergencyContactName: member.emergencyContactName || '',
      emergencyContactPhone: member.emergencyContactPhone || '',
      warehouseId: (member as any).assignedWarehouseId || member.warehouseId || null,
      shift: member.shift || 'General (09:00 AM - 06:00 PM)',
      joiningDate: member.joiningDate
        ? member.joiningDate.substring(0, 10)
        : new Date().toISOString().substring(0, 10),
      qualifications: member.qualifications || '',
      status: member.status || 'Active',
      notes: member.notes || '',
      isActive: member.isActive,
    };

    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.isEditing = false;
    this.editingId = null;
    this.cdr.detectChanges();
  }

  saveFaculty(): void {
    const code = (this.formModel.employeeCode || '').trim().toUpperCase();
    const name = (this.formModel.fullName || '').trim();
    const designation = (this.formModel.designation || '').trim();

    if (!code) {
      this.toasterService.warn('Employee Code is required (e.g. FAC-101).', 'Validation');
      return;
    }
    if (!name) {
      this.toasterService.warn('Full Name is required.', 'Validation');
      return;
    }
    if (!designation) {
      this.toasterService.warn('Designation is required.', 'Validation');
      return;
    }

    // Check duplicate employee code
    const isDuplicate = this.facultyMembers.some(
      (m) => m.employeeCode.toUpperCase() === code && m.id !== this.editingId
    );
    if (isDuplicate) {
      this.toasterService.warn(`Employee code '${code}' already exists. Please choose a unique code.`, 'Duplicate Code');
      return;
    }

    this.isSaving = true;
    const payload: CreateUpdateFacultyMemberDto = {
      ...this.formModel,
      employeeCode: code,
      fullName: name,
      designation,
      department: (this.formModel.department || '').trim() || null,
      email: (this.formModel.email || '').trim() || null,
      phoneNumber: (this.formModel.phoneNumber || '').trim() || null,
      emergencyContactName: (this.formModel.emergencyContactName || '').trim() || null,
      emergencyContactPhone: (this.formModel.emergencyContactPhone || '').trim() || null,
      warehouseId: this.formModel.warehouseId || null,
      assignedWarehouseId: this.formModel.warehouseId || null,
      shift: this.formModel.shift || null,
      joiningDate: this.formModel.joiningDate ? new Date(this.formModel.joiningDate).toISOString() : null,
      qualifications: (this.formModel.qualifications || '').trim() || null,
      status: this.formModel.status || 'Active',
      notes: (this.formModel.notes || '').trim() || null,
      isActive: this.formModel.isActive !== false,
    };

    if (this.isEditing && this.editingId) {
      this.facultyService.update(this.editingId, payload).subscribe({
        next: (updated) => {
          this.isSaving = false;
          this.toasterService.success(`Faculty member ${updated.fullName} updated successfully!`, 'Success');
          this.closeModal();
          this.loadFacultyMembers();
        },
        error: (err) => {
          console.error('Error updating faculty member:', err);
          const msg = err?.error?.error?.message || err?.message || 'Failed to update faculty member';
          this.toasterService.error(msg, 'Error');
          this.isSaving = false;
          this.cdr.detectChanges();
        },
      });
    } else {
      this.facultyService.create(payload).subscribe({
        next: (created) => {
          this.isSaving = false;
          this.toasterService.success(`Faculty member ${created.fullName} registered successfully!`, 'Success');
          this.closeModal();
          this.loadFacultyMembers();
        },
        error: (err) => {
          console.error('Error creating faculty member:', err);
          const msg = err?.error?.error?.message || err?.message || 'Failed to create faculty member';
          this.toasterService.error(msg, 'Error');
          this.isSaving = false;
          this.cdr.detectChanges();
        },
      });
    }
  }

  deleteFaculty(member: FacultyMemberDto): void {
    if (!member.id) return;
    if (!confirm(`Are you sure you want to remove faculty member '${member.fullName}' (${member.employeeCode})?`)) {
      return;
    }

    this.facultyService.delete(member.id).subscribe({
      next: () => {
        this.toasterService.success(`Faculty member '${member.fullName}' removed successfully.`, 'Deleted');
        this.loadFacultyMembers();
      },
      error: (err) => {
        console.error('Error deleting faculty member:', err);
        const msg = err?.error?.error?.message || err?.message || 'Failed to delete faculty member.';
        this.toasterService.error(msg, 'Error');
      },
    });
  }

  toggleActive(member: FacultyMemberDto): void {
    if (!member.id) return;
    const newActiveState = !member.isActive;
    const payload: CreateUpdateFacultyMemberDto = {
      employeeCode: member.employeeCode,
      fullName: member.fullName,
      designation: member.designation,
      department: member.department,
      email: member.email,
      phoneNumber: member.phoneNumber,
      emergencyContactName: member.emergencyContactName,
      emergencyContactPhone: member.emergencyContactPhone,
      warehouseId: member.warehouseId,
      shift: member.shift,
      joiningDate: member.joiningDate,
      qualifications: member.qualifications,
      status: newActiveState ? 'Active' : 'Inactive',
      notes: member.notes,
      isActive: newActiveState,
    };

    this.facultyService.update(member.id, payload).subscribe({
      next: () => {
        member.isActive = newActiveState;
        member.status = newActiveState ? 'Active' : 'Inactive';
        this.updateMetrics();
        this.applyFilters();
        this.toasterService.info(
          `Status changed to ${newActiveState ? 'Active' : 'Inactive'} for ${member.fullName}`,
          'Status Updated'
        );
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to toggle status:', err);
        this.toasterService.error('Failed to change active status.', 'Error');
      },
    });
  }

  getInitials(name: string): string {
    if (!name) return 'FM';
    const parts = name.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }

  getAvatarColor(name: string): string {
    const colors = [
      '#2563eb', // blue
      '#7c3aed', // purple
      '#059669', // emerald
      '#d97706', // amber
      '#dc2626', // red
      '#0891b2', // cyan
      '#4f46e5', // indigo
      '#db2777', // pink
    ];
    let hash = 0;
    for (let i = 0; i < (name || '').length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const index = Math.abs(hash) % colors.length;
    return colors[index];
  }

  private generateNextEmployeeCode(): string {
    if (!this.facultyMembers.length) return 'FAC-101';
    const codes = this.facultyMembers
      .map((m) => {
        const match = m.employeeCode.match(/\d+/);
        return match ? parseInt(match[0], 10) : 0;
      })
      .filter((n) => n > 0);
    const maxNum = codes.length > 0 ? Math.max(...codes) : 100;
    return `FAC-${maxNum + 1}`;
  }
}
