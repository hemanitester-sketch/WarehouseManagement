import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ToasterService } from '@abp/ng.theme.shared';
import { RouterModule } from '@angular/router';
import { FacultyMemberService } from '../proxy/faculty/faculty-member.service';
import { FacultyMemberLookupDto } from '../proxy/faculty/models';
import { 
  WarehouseService, 
  WarehouseZoneService, 
  WarehouseLocationService 
} from '../proxy/warehouses';
import { 
  WarehouseDto, 
  WarehouseZoneDto, 
  WarehouseLocationDto, 
  CreateUpdateWarehouseDto,
  CreateUpdateWarehouseZoneDto,
  CreateUpdateWarehouseLocationDto
} from '../proxy/warehouses/models';
import { WarehouseType, warehouseTypeOptions } from '../proxy/warehouses/warehouse-type.enum';
import { WarehouseZoneStorageType, warehouseZoneStorageTypeOptions } from '../proxy/warehouses/warehouse-zone-storage-type.enum';
import { WarehouseLocationType, warehouseLocationTypeOptions } from '../proxy/warehouses/warehouse-location-type.enum';
import { WarehouseLocationStatus, warehouseLocationStatusOptions } from '../proxy/warehouses/warehouse-location-status.enum';

@Component({
  selector: 'app-warehouses',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './warehouses.component.html',
  styleUrls: ['./warehouses.component.scss']
})
export class WarehousesComponent implements OnInit {
  private warehouseService = inject(WarehouseService);
  private zoneService = inject(WarehouseZoneService);
  private locationService = inject(WarehouseLocationService);
  private toasterService = inject(ToasterService);
  private facultyService = inject(FacultyMemberService);

  facultyLookups: FacultyMemberLookupDto[] = [];
  private cdr = inject(ChangeDetectorRef);

  warehouses: WarehouseDto[] = [];
  filteredWarehouses: WarehouseDto[] = [];
  isLoading = false;
  searchQuery = '';
  selectedTypeFilter: number | null = null;
  selectedStatusTab: 'all' | 'operational' | 'inactive' | 'DistributionCenter' | 'RetailBackroom' | 'FulfillmentCenter' = 'all';
  sortBy: 'name' | 'code' | 'areaDesc' | 'areaAsc' = 'name';
  viewMode: 'grid' | 'table' = 'grid';
  editingWarehouseId: string | null = null;

  // Pagination State (Default 50 per page as requested)
  currentPage = 1;
  pageSize = 50;
  pageSizeOptions = [10, 25, 50, 100];

  // In-memory static cache for instantaneous navigation (0ms delay)
  private static cachedWarehouses: WarehouseDto[] | null = null;
  private static cachedZonesCount = 0;
  private static cachedLocationsCount = 0;

  totalZonesCount = 0;
  totalLocationsCount = 0;

  // Selected warehouse for zone & bin inspection
  selectedWarehouse: WarehouseDto | null = null;
  selectedWarehouseZones: WarehouseZoneDto[] = [];
  selectedWarehouseLocations: WarehouseLocationDto[] = [];
  isLoadingDetails = false;

  // Detail Modal Tabs & Filters
  activeDetailTab: 'zones' | 'bins' | 'guide' = 'zones';
  selectedZoneFilter: string = 'all';
  binSearchQuery: string = '';
  zoneSearchQuery: string = '';
  selectedZoneTypeFilter: string = 'all';
  binCurrentPage = 1;
  binPageSize = 15;
  isQuickSettingUp: boolean = false;

  // Modals
  isCreateWarehouseModalOpen = false;
  isCreateZoneModalOpen = false;
  isCreateLocationModalOpen = false;

  // Warehouse Form Model
  newWarehouse: CreateUpdateWarehouseDto = {
    code: '',
    name: '',
    type: WarehouseType.DistributionCenter,
    address: '',
    city: '',
    state: '',
    postalCode: '',
    country: 'India',
    managerId: null,
    contactPerson: '',
    contactPhone: '+91 98765 43210',
    contactEmail: 'operations@warehousemanagement.com',
    totalAreaSqFt: 50000,
    isActive: true
  };

  // Zone Form Model
  newZone: CreateUpdateWarehouseZoneDto = {
    warehouseId: '',
    code: '',
    name: '',
    storageType: WarehouseZoneStorageType.BulkReserve,
    temperatureMinCelsius: null,
    temperatureMaxCelsius: null,
    isActive: true
  };

  // Location Form Model
  newLocation: CreateUpdateWarehouseLocationDto = {
    warehouseId: '',
    zoneId: '',
    barcode: '',
    aisle: 'A01',
    rack: 'R01',
    shelf: 'S1',
    bin: 'B01',
    locationType: WarehouseLocationType.PalletRack,
    status: WarehouseLocationStatus.Available,
    maxWeightKg: 1000,
    maxVolumeCm3: 5000000,
    isPickFace: true,
    isActive: true
  };

  // Options from Proxy
  warehouseTypes = warehouseTypeOptions;
  zoneStorageTypes = warehouseZoneStorageTypeOptions;
  locationTypes = warehouseLocationTypeOptions;
  locationStatuses = warehouseLocationStatusOptions;

  ngOnInit(): void {
    // 1. Instant Cache Initialization: Render instantly if cached
    const cached = WarehousesComponent.cachedWarehouses || this.loadSessionCache();
    if (cached && cached.length > 0) {
      this.warehouses = cached;
      this.totalZonesCount = WarehousesComponent.cachedZonesCount;
      this.totalLocationsCount = WarehousesComponent.cachedLocationsCount;
      this.applyFilter();
      this.isLoading = false;
    } else {
      this.isLoading = true;
    }

    // 2. Stale-While-Revalidate: fetch fresh data from backend
    this.loadWarehouses();
    this.loadNetworkCounts();
    this.loadFacultyLookups();
  }

  loadWarehouses(): void {
    if (!this.warehouses || this.warehouses.length === 0) {
      this.isLoading = true;
    }

    this.warehouseService.getList({
      filter: '',
      skipCount: 0,
      maxResultCount: 50
    }).subscribe({
      next: (res) => {
        this.warehouses = res.items || [];
        WarehousesComponent.cachedWarehouses = this.warehouses;
        this.saveSessionCache(this.warehouses);
        this.applyFilter();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load warehouses', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadNetworkCounts(): void {
    this.zoneService.getList({ maxResultCount: 1, skipCount: 0 }).subscribe({
      next: (res) => {
        this.totalZonesCount = res.totalCount || 0;
        WarehousesComponent.cachedZonesCount = this.totalZonesCount;
        this.cdr.detectChanges();
      },
      error: () => {}
    });

    this.locationService.getList({ maxResultCount: 1, skipCount: 0 }).subscribe({
      next: (res) => {
        this.totalLocationsCount = res.totalCount || 0;
        WarehousesComponent.cachedLocationsCount = this.totalLocationsCount;
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  private loadSessionCache(): WarehouseDto[] | null {
    try {
      const data = sessionStorage.getItem('wms_warehouses_cache');
      return data ? JSON.parse(data) : null;
    } catch {
      return null;
    }
  }

  private saveSessionCache(items: WarehouseDto[]): void {
    try {
      sessionStorage.setItem('wms_warehouses_cache', JSON.stringify(items));
    } catch {
      // ignore
    }
  }

  applyFilter(): void {
    let list = [...this.warehouses];

    // Status / Type tab filter
    if (this.selectedStatusTab === 'operational') {
      list = list.filter(w => w.isActive);
    } else if (this.selectedStatusTab === 'inactive') {
      list = list.filter(w => !w.isActive);
    } else if (this.selectedStatusTab === 'DistributionCenter') {
      list = list.filter(w => w.type === WarehouseType.DistributionCenter);
    } else if (this.selectedStatusTab === 'RetailBackroom') {
      list = list.filter(w => w.type === WarehouseType.RetailBackroom);
    } else if (this.selectedStatusTab === 'FulfillmentCenter') {
      list = list.filter(w => w.type === WarehouseType.FulfillmentCenter);
    }

    // Type dropdown filter
    if (this.selectedTypeFilter !== null && this.selectedTypeFilter !== undefined) {
      list = list.filter(w => w.type === this.selectedTypeFilter);
    }

    // Search query filter
    const q = this.searchQuery?.trim().toLowerCase();
    if (q) {
      list = list.filter(w => 
        w.code?.toLowerCase().includes(q) ||
        w.name?.toLowerCase().includes(q) ||
        w.city?.toLowerCase().includes(q) ||
        w.state?.toLowerCase().includes(q) ||
        w.contactPerson?.toLowerCase().includes(q) ||
        w.managerName?.toLowerCase().includes(q)
      );
    }

    // Sort
    if (this.sortBy === 'name') {
      list.sort((a, b) => (a.name || '').localeCompare(b.name || ''));
    } else if (this.sortBy === 'code') {
      list.sort((a, b) => (a.code || '').localeCompare(b.code || ''));
    } else if (this.sortBy === 'areaDesc') {
      list.sort((a, b) => (b.totalAreaSqFt || 0) - (a.totalAreaSqFt || 0));
    } else if (this.sortBy === 'areaAsc') {
      list.sort((a, b) => (a.totalAreaSqFt || 0) - (b.totalAreaSqFt || 0));
    }

    this.filteredWarehouses = list;
    this.currentPage = 1;
  }

  get paginatedWarehouses(): WarehouseDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredWarehouses.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredWarehouses.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    return this.filteredWarehouses.length === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredWarehouses.length);
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

  getCapacityPercent(wh: WarehouseDto): number {
    const area = wh.totalAreaSqFt || 50000;
    const base = (area % 35) + 52;
    return Math.min(Math.max(base, 45), 88);
  }

  countOperational(): number {
    return this.warehouses.filter(w => w.isActive).length;
  }

  trackById(index: number, item: WarehouseDto): string {
    return item.id || index.toString();
  }

  getTypeLabel(type: WarehouseType | undefined): string {
    const found = this.warehouseTypes.find(t => t.value === type);
    return found ? found.key : 'Distribution Center';
  }

  getStorageTypeLabel(type: WarehouseZoneStorageType | undefined): string {
    const found = this.zoneStorageTypes.find(t => t.value === type);
    return found ? found.key : 'Bulk Reserve';
  }

  getLocationTypeLabel(type: WarehouseLocationType | undefined): string {
    const found = this.locationTypes.find(t => t.value === type);
    return found ? found.key : 'Pallet Rack';
  }

  getLocationStatusLabel(status: WarehouseLocationStatus | undefined): string {
    switch (status) {
      case WarehouseLocationStatus.Available: return 'Available';
      case WarehouseLocationStatus.Occupied: return 'Occupied';
      case WarehouseLocationStatus.Blocked: return 'Blocked';
      case WarehouseLocationStatus.UnderMaintenance: return 'Maintenance';
      default: return 'Available';
    }
  }

  getLocationStatusBadge(status: WarehouseLocationStatus | undefined): string {
    switch (status) {
      case WarehouseLocationStatus.Available: return 'wms-badge-success';
      case WarehouseLocationStatus.Occupied: return 'wms-badge-info';
      case WarehouseLocationStatus.Blocked: return 'wms-badge-danger';
      case WarehouseLocationStatus.UnderMaintenance: return 'wms-badge-warning';
      default: return 'wms-badge-primary';
    }
  }

  getZoneByLocation(loc: WarehouseLocationDto): WarehouseZoneDto | undefined {
    return this.selectedWarehouseZones.find(z => z.id === loc.zoneId);
  }

  // Inspection Workspace
  inspectWarehouse(wh: WarehouseDto): void {
    this.selectedWarehouse = wh;
    this.activeDetailTab = 'zones';
    this.selectedZoneFilter = 'all';
    this.binSearchQuery = '';
    this.loadWarehouseDetails(wh.id!);
    this.cdr.detectChanges();
  }

  closeInspection(): void {
    this.selectedWarehouse = null;
    this.selectedWarehouseZones = [];
    this.selectedWarehouseLocations = [];
    this.activeDetailTab = 'zones';
    this.cdr.detectChanges();
  }

  loadWarehouseDetails(warehouseId: string): void {
    this.isLoadingDetails = true;
    this.cdr.detectChanges();
    forkJoin({
      zones: this.zoneService.getList({ warehouseId, maxResultCount: 50, skipCount: 0 }),
      locations: this.locationService.getList({ warehouseId, maxResultCount: 100, skipCount: 0 })
    }).subscribe({
      next: (res) => {
        this.selectedWarehouseZones = res.zones.items || [];
        this.selectedWarehouseLocations = res.locations.items || [];
        this.isLoadingDetails = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load facility zones and bins', err);
        this.isLoadingDetails = false;
        this.cdr.detectChanges();
      }
    });
  }

  get filteredLocations(): WarehouseLocationDto[] {
    return this.selectedWarehouseLocations.filter(loc => {
      const matchesZone = this.selectedZoneFilter === 'all' || loc.zoneId === this.selectedZoneFilter;
      const matchesSearch = !this.binSearchQuery || 
        loc.barcode?.toLowerCase().includes(this.binSearchQuery.toLowerCase()) ||
        loc.aisle?.toLowerCase().includes(this.binSearchQuery.toLowerCase()) ||
        loc.rack?.toLowerCase().includes(this.binSearchQuery.toLowerCase());
      return matchesZone && matchesSearch;
    });
  }

  getBinsCountForZone(zoneId: string | undefined): number {
    if (!zoneId) return 0;
    return this.selectedWarehouseLocations.filter(loc => loc.zoneId === zoneId).length;
  }

  get filteredZones(): WarehouseZoneDto[] {
    return this.selectedWarehouseZones.filter(zone => {
      const q = (this.zoneSearchQuery || '').trim().toLowerCase();
      const matchesSearch = !q ||
        zone.code?.toLowerCase().includes(q) ||
        zone.name?.toLowerCase().includes(q) ||
        this.getStorageTypeLabel(zone.storageType)?.toLowerCase().includes(q);

      let matchesType = true;
      if (this.selectedZoneTypeFilter === 'cold') {
        matchesType = zone.temperatureMinCelsius !== null || zone.storageType === WarehouseZoneStorageType.ColdStorage;
      } else if (this.selectedZoneTypeFilter === 'ambient') {
        matchesType = zone.temperatureMinCelsius === null && zone.storageType !== WarehouseZoneStorageType.ColdStorage;
      } else if (this.selectedZoneTypeFilter !== 'all') {
        matchesType = zone.storageType === Number(this.selectedZoneTypeFilter);
      }

      return matchesSearch && matchesType;
    });
  }

  get coldZonesCount(): number {
    return this.selectedWarehouseZones.filter(z => z.temperatureMinCelsius !== null || z.storageType === WarehouseZoneStorageType.ColdStorage).length;
  }

  get ambientZonesCount(): number {
    return this.selectedWarehouseZones.length - this.coldZonesCount;
  }

  get totalBinsInSelectedWarehouse(): number {
    return this.selectedWarehouseLocations.length;
  }

  get paginatedLocations(): WarehouseLocationDto[] {
    const start = (this.binCurrentPage - 1) * this.binPageSize;
    return this.filteredLocations.slice(start, start + this.binPageSize);
  }

  get binTotalPages(): number {
    return Math.ceil(this.filteredLocations.length / this.binPageSize) || 1;
  }

  get binPaginationStart(): number {
    return this.filteredLocations.length === 0 ? 0 : (this.binCurrentPage - 1) * this.binPageSize + 1;
  }

  get binPaginationEnd(): number {
    return Math.min(this.binCurrentPage * this.binPageSize, this.filteredLocations.length);
  }

  binPrevPage(): void {
    if (this.binCurrentPage > 1) this.binCurrentPage--;
  }

  binNextPage(): void {
    if (this.binCurrentPage < this.binTotalPages) this.binCurrentPage++;
  }

  onBinPageSizeChange(newSize: number): void {
    this.binPageSize = Number(newSize);
    this.binCurrentPage = 1;
  }

  openCreateLocationInZone(zone: WarehouseZoneDto): void {
    if (!this.selectedWarehouse) return;
    const nextBinNum = (this.getBinsCountForZone(zone.id) + 1).toString().padStart(2, '0');
    this.newLocation = {
      warehouseId: this.selectedWarehouse.id!,
      zoneId: zone.id!,
      barcode: `LOC-${this.selectedWarehouse.code}-${zone.code}-A01-R01-S1-B${nextBinNum}`,
      aisle: 'A01',
      rack: 'R01',
      shelf: 'S1',
      bin: `B${nextBinNum}`,
      locationType: WarehouseLocationType.PalletRack,
      status: WarehouseLocationStatus.Available,
      maxWeightKg: 1200,
      maxVolumeCm3: 5000000,
      isPickFace: true,
      isActive: true
    };
    this.isCreateLocationModalOpen = true;
  }

  deleteZone(zone: WarehouseZoneDto): void {
    const binsCount = this.getBinsCountForZone(zone.id);
    if (binsCount > 0) {
      this.toasterService.warn(
        `Zone '${zone.name}' contains ${binsCount} mapped bins. Please delete or reassign bins first.`,
        'Cannot Delete Zone'
      );
      return;
    }

    if (!confirm(`Are you sure you want to delete zone '${zone.name}' (${zone.code})?`)) return;

    this.zoneService.delete(zone.id!).subscribe({
      next: () => {
        this.toasterService.success(`Zone '${zone.name}' deleted.`, 'Success');
        this.loadWarehouseDetails(this.selectedWarehouse!.id!);
        this.loadNetworkCounts();
      },
      error: () => {
        this.toasterService.error('Failed to delete zone', 'Error');
      }
    });
  }

  deleteLocation(loc: WarehouseLocationDto): void {
    if (!confirm(`Are you sure you want to delete bin location '${loc.barcode}'?`)) return;

    this.locationService.delete(loc.id!).subscribe({
      next: () => {
        this.toasterService.success(`Location '${loc.barcode}' deleted.`, 'Success');
        this.loadWarehouseDetails(this.selectedWarehouse!.id!);
        this.loadNetworkCounts();
      },
      error: () => {
        this.toasterService.error('Failed to delete bin location', 'Error');
      }
    });
  }

  copyBarcodeToClipboard(barcode: string | undefined): void {
    if (!barcode) return;
    if (navigator?.clipboard?.writeText) {
      navigator.clipboard.writeText(barcode).then(() => {
        this.toasterService.success(`Barcode '${barcode}' copied to clipboard!`, 'Copied');
      });
    } else {
      this.toasterService.info(barcode, 'Barcode');
    }
  }

  // 1-Click Quick Setup for New Facilities
  quickSetupDefaultZones(): void {
    if (!this.selectedWarehouse) return;
    this.isQuickSettingUp = true;

    const ambientZone: CreateUpdateWarehouseZoneDto = {
      warehouseId: this.selectedWarehouse.id!,
      code: 'ZN-AMB',
      name: 'Ambient Dry Pallet Storage',
      storageType: WarehouseZoneStorageType.BulkReserve,
      temperatureMinCelsius: null,
      temperatureMaxCelsius: null,
      isActive: true
    };

    const coldZone: CreateUpdateWarehouseZoneDto = {
      warehouseId: this.selectedWarehouse.id!,
      code: 'ZN-COLD',
      name: 'Cold Chain Pharma Chamber',
      storageType: WarehouseZoneStorageType.ColdStorage,
      temperatureMinCelsius: 2,
      temperatureMaxCelsius: 8,
      isActive: true
    };

    this.zoneService.create(ambientZone).subscribe({
      next: (createdAmbient) => {
        this.zoneService.create(coldZone).subscribe({
          next: () => {
            // Seed a starter scannable bin in Ambient Zone
            const starterBin: CreateUpdateWarehouseLocationDto = {
              warehouseId: this.selectedWarehouse!.id!,
              zoneId: createdAmbient.id!,
              barcode: `LOC-${this.selectedWarehouse!.code}-AMB-A01-R01-S1-B01`,
              aisle: 'A01',
              rack: 'R01',
              shelf: 'S1',
              bin: 'B01',
              locationType: WarehouseLocationType.PalletRack,
              status: WarehouseLocationStatus.Available,
              maxWeightKg: 1200,
              maxVolumeCm3: 5000000,
              isPickFace: true,
              isActive: true
            };

            this.locationService.create(starterBin).subscribe({
              next: () => {
                this.isQuickSettingUp = false;
                this.loadWarehouseDetails(this.selectedWarehouse!.id!);
                this.loadNetworkCounts();
              },
              error: () => {
                this.isQuickSettingUp = false;
                this.loadWarehouseDetails(this.selectedWarehouse!.id!);
                this.loadNetworkCounts();
              }
            });
          },
          error: () => {
            this.isQuickSettingUp = false;
            this.loadWarehouseDetails(this.selectedWarehouse!.id!);
          }
        });
      },
      error: () => {
        this.isQuickSettingUp = false;
      }
    });
  }

  // Create Warehouse
  loadFacultyLookups(): void {
    this.facultyService.getLookup().subscribe({
      next: (res: any) => {
        this.facultyLookups = res?.items || (Array.isArray(res) ? res : []);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load faculty lookups for warehouses:', err);
      }
    });
  }

  onManagerSelected(managerId: string | null): void {
    if (!managerId) {
      this.newWarehouse.managerId = null;
      return;
    }
    const found = this.facultyLookups.find(f => f.id === managerId);
    if (found) {
      this.newWarehouse.managerId = found.id;
      this.newWarehouse.contactPerson = found.fullName;
      if (found.phoneNumber) {
        this.newWarehouse.contactPhone = found.phoneNumber;
      }
      if (found.email) {
        this.newWarehouse.contactEmail = found.email;
      }
    }
  }

  openCreateWarehouseModal(): void {
    this.editingWarehouseId = null;
    this.loadFacultyLookups();
    this.newWarehouse = {
      code: '',
      name: '',
      type: WarehouseType.DistributionCenter,
      address: '',
      city: '',
      state: '',
      postalCode: '',
      country: 'India',
      managerId: null,
      contactPerson: '',
      contactPhone: '+91 98765 43210',
      contactEmail: 'operations@warehousemanagement.com',
      totalAreaSqFt: 50000,
      isActive: true
    };
    this.isCreateWarehouseModalOpen = true;
    this.cdr.detectChanges();
  }

  openEditWarehouseModal(wh: WarehouseDto): void {
    this.loadFacultyLookups();
    this.editingWarehouseId = wh.id || null;
    this.newWarehouse = {
      code: wh.code || '',
      name: wh.name || '',
      type: wh.type ?? WarehouseType.DistributionCenter,
      address: wh.address || '',
      city: wh.city || '',
      state: wh.state || '',
      postalCode: wh.postalCode || '',
      country: wh.country || 'India',
      managerId: wh.managerId || null,
      contactPerson: wh.contactPerson || wh.managerName || '',
      contactPhone: wh.contactPhone || '+91 98765 43210',
      contactEmail: wh.contactEmail || 'operations@warehousemanagement.com',
      totalAreaSqFt: wh.totalAreaSqFt || 50000,
      isActive: wh.isActive ?? true
    };
    this.isCreateWarehouseModalOpen = true;
    this.cdr.detectChanges();
  }

  saveWarehouse(): void {
    if (!this.newWarehouse.code?.trim()) {
      this.toasterService.warn('Warehouse Code is required.', 'Validation Warning');
      return;
    }
    if (!this.newWarehouse.name?.trim()) {
      this.toasterService.warn('Warehouse Name is required.', 'Validation Warning');
      return;
    }
    if (!this.newWarehouse.totalAreaSqFt || this.newWarehouse.totalAreaSqFt <= 0) {
      this.toasterService.warn('Total Floor Area must be greater than 0 sq.ft.', 'Validation Warning');
      return;
    }
    if (!this.newWarehouse.address?.trim()) {
      this.toasterService.warn('Street Address is required for logistics routing.', 'Validation Warning');
      return;
    }
    if (!this.newWarehouse.city?.trim()) {
      this.toasterService.warn('City is required.', 'Validation Warning');
      return;
    }
    if (!this.newWarehouse.state?.trim()) {
      this.toasterService.warn('State is required.', 'Validation Warning');
      return;
    }
    if (!this.newWarehouse.postalCode?.trim()) {
      this.toasterService.warn('Postal Code (Pincode) is required.', 'Validation Warning');
      return;
    }
    if (!this.newWarehouse.contactPerson?.trim()) {
      this.toasterService.warn('Facility Manager name is required.', 'Validation Warning');
      return;
    }
    if (!this.newWarehouse.contactPhone?.trim()) {
      this.toasterService.warn('Contact Phone number is required.', 'Validation Warning');
      return;
    }

    const codeTrimmed = this.newWarehouse.code.trim().toUpperCase();
    const nameTrimmed = this.newWarehouse.name.trim().toLowerCase();

    // Check duplicate code (excluding currently edited warehouse)
    const codeExists = this.warehouses.some(w => 
      w.id !== this.editingWarehouseId && 
      w.code?.trim().toUpperCase() === codeTrimmed
    );
    if (codeExists) {
      this.toasterService.warn(
        `A warehouse with code '${this.newWarehouse.code}' already exists. Please choose a different warehouse code.`,
        'Duplicate Warehouse Code'
      );
      return;
    }

    // Check duplicate name (excluding currently edited warehouse)
    const nameExists = this.warehouses.some(w => 
      w.id !== this.editingWarehouseId && 
      w.name?.trim().toLowerCase() === nameTrimmed
    );
    if (nameExists) {
      this.toasterService.warn(
        `A warehouse with name '${this.newWarehouse.name}' already exists. Please choose a different warehouse name.`,
        'Duplicate Warehouse Name'
      );
      return;
    }

    if (this.editingWarehouseId) {
      this.warehouseService.update(this.editingWarehouseId, this.newWarehouse).subscribe({
        next: () => {
          this.isCreateWarehouseModalOpen = false;
          this.editingWarehouseId = null;
          this.toasterService.success('Warehouse updated successfully!', 'Updated');
          this.loadWarehouses();
          this.loadNetworkCounts();
        },
        error: (err) => {
          console.error('Failed to update warehouse', err);
          const message = err?.error?.error?.message || err?.message || 'Failed to update warehouse';
          this.toasterService.error(message, 'Update Error');
        }
      });
    } else {
      this.warehouseService.create(this.newWarehouse).subscribe({
        next: () => {
          this.isCreateWarehouseModalOpen = false;
          this.editingWarehouseId = null;
          this.toasterService.success('Warehouse registered successfully!', 'Success');
          this.loadWarehouses();
          this.loadNetworkCounts();
        },
        error: (err) => {
          console.error('Failed to create warehouse', err);
          const message = err?.error?.error?.message || err?.message || 'Failed to create warehouse';
          this.toasterService.error(message, 'Registration Error');
        }
      });
    }
  }

  // Create Zone
  openCreateZoneModal(): void {
    if (!this.selectedWarehouse) return;
    const nextIdx = this.selectedWarehouseZones.length + 1;
    this.newZone = {
      warehouseId: this.selectedWarehouse.id!,
      code: `ZN-0${nextIdx}`,
      name: `Storage Zone 0${nextIdx}`,
      storageType: WarehouseZoneStorageType.BulkReserve,
      temperatureMinCelsius: null,
      temperatureMaxCelsius: null,
      isActive: true
    };
    this.isCreateZoneModalOpen = true;
  }

  saveZone(): void {
    if (!this.newZone.code || !this.newZone.name) return;

    const codeTrimmed = this.newZone.code.trim().toUpperCase();
    const nameTrimmed = this.newZone.name.trim().toLowerCase();

    // Check duplicate zone code within current warehouse
    const codeExists = this.selectedWarehouseZones.some(z => z.code?.trim().toUpperCase() === codeTrimmed);
    if (codeExists) {
      this.toasterService.warn(
        `A zone with code '${this.newZone.code}' already exists in this warehouse.`,
        'Duplicate Zone Code'
      );
      return;
    }

    // Check duplicate zone name within current warehouse
    const nameExists = this.selectedWarehouseZones.some(z => z.name?.trim().toLowerCase() === nameTrimmed);
    if (nameExists) {
      this.toasterService.warn(
        `A zone with name '${this.newZone.name}' already exists in this warehouse.`,
        'Duplicate Zone Name'
      );
      return;
    }

    this.zoneService.create(this.newZone).subscribe({
      next: () => {
        this.isCreateZoneModalOpen = false;
        this.toasterService.success('Storage zone created successfully!', 'Success');
        this.loadWarehouseDetails(this.selectedWarehouse!.id!);
        this.loadNetworkCounts();
      },
      error: (err) => {
        console.error('Failed to create zone', err);
        const message = err?.error?.error?.message || err?.message || 'Failed to create zone';
        this.toasterService.error(message, 'Zone Error');
      }
    });
  }

  // Create Location
  openCreateLocationModal(): void {
    if (!this.selectedWarehouse) return;
    if (!this.selectedWarehouseZones.length) {
      // If no zones exist yet, redirect user to create a zone first
      this.activeDetailTab = 'zones';
      this.openCreateZoneModal();
      return;
    }

    const defaultZone = this.selectedWarehouseZones[0];
    this.newLocation = {
      warehouseId: this.selectedWarehouse.id!,
      zoneId: defaultZone.id!,
      barcode: `LOC-${this.selectedWarehouse.code}-${defaultZone.code}-A01-R01-S1-B01`,
      aisle: 'A01',
      rack: 'R01',
      shelf: 'S1',
      bin: 'B01',
      locationType: WarehouseLocationType.PalletRack,
      status: WarehouseLocationStatus.Available,
      maxWeightKg: 1200,
      maxVolumeCm3: 5000000,
      isPickFace: true,
      isActive: true
    };
    this.isCreateLocationModalOpen = true;
  }

  updateGeneratedBarcode(): void {
    const zone = this.selectedWarehouseZones.find(z => z.id === this.newLocation.zoneId);
    const zCode = zone ? zone.code : 'ZN';
    this.newLocation.barcode = `LOC-${this.selectedWarehouse?.code}-${zCode}-${this.newLocation.aisle}-${this.newLocation.rack}-${this.newLocation.shelf}-${this.newLocation.bin}`;
  }

  saveLocation(): void {
    if (!this.newLocation.barcode || !this.newLocation.zoneId) return;
    this.locationService.create(this.newLocation).subscribe({
      next: () => {
        this.isCreateLocationModalOpen = false;
        this.toasterService.success('Bin location created successfully!', 'Success');
        this.loadWarehouseDetails(this.selectedWarehouse!.id!);
        this.loadNetworkCounts();
      },
      error: (err) => console.error('Failed to create location', err)
    });
  }
}
