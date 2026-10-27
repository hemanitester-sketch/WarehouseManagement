import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ToasterService } from '@abp/ng.theme.shared';
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
  imports: [CommonModule, FormsModule],
  templateUrl: './warehouses.component.html',
  styleUrls: ['./warehouses.component.scss']
})
export class WarehousesComponent implements OnInit {
  private warehouseService = inject(WarehouseService);
  private zoneService = inject(WarehouseZoneService);
  private locationService = inject(WarehouseLocationService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  warehouses: WarehouseDto[] = [];
  filteredWarehouses: WarehouseDto[] = [];
  isLoading = false;
  searchQuery = '';
  selectedTypeFilter: number | null = null;

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
    contactPerson: 'Hub Operations Manager',
    contactPhone: '+91 98765 43210',
    contactEmail: 'operations@apexlogistics.com',
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
    this.filteredWarehouses = this.warehouses.filter(w => {
      const matchesSearch = !this.searchQuery || 
        w.code?.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        w.name?.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        w.city?.toLowerCase().includes(this.searchQuery.toLowerCase());

      const matchesType = this.selectedTypeFilter === null || w.type === this.selectedTypeFilter;

      return matchesSearch && matchesType;
    });
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
  openCreateWarehouseModal(): void {
    this.newWarehouse = {
      code: '',
      name: '',
      type: WarehouseType.DistributionCenter,
      address: '',
      city: '',
      state: '',
      postalCode: '',
      country: 'India',
      contactPerson: 'Facility Operations Head',
      contactPhone: '+91 98765 43210',
      contactEmail: 'operations@apexlogistics.com',
      totalAreaSqFt: 50000,
      isActive: true
    };
    this.isCreateWarehouseModalOpen = true;
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

    // Check duplicate code
    const codeExists = this.warehouses.some(w => w.code?.trim().toUpperCase() === codeTrimmed);
    if (codeExists) {
      this.toasterService.warn(
        `A warehouse with code '${this.newWarehouse.code}' already exists. Please choose a different warehouse code.`,
        'Duplicate Warehouse Code'
      );
      return;
    }

    // Check duplicate name
    const nameExists = this.warehouses.some(w => w.name?.trim().toLowerCase() === nameTrimmed);
    if (nameExists) {
      this.toasterService.warn(
        `A warehouse with name '${this.newWarehouse.name}' already exists. Please choose a different warehouse name.`,
        'Duplicate Warehouse Name'
      );
      return;
    }

    this.warehouseService.create(this.newWarehouse).subscribe({
      next: () => {
        this.isCreateWarehouseModalOpen = false;
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
