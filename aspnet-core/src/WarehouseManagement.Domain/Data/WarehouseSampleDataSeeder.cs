using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Data;

public class WarehouseSampleDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<WarehouseZone, Guid> _zoneRepository;
    private readonly IRepository<WarehouseLocation, Guid> _locationRepository;
    private readonly IRepository<WarehouseManagement.Products.Product, Guid> _productRepository;
    private readonly IRepository<WarehouseManagement.Inventory.InventoryItem, Guid> _inventoryRepository;
    private readonly IRepository<WarehouseManagement.Inventory.InventoryTransaction, Guid> _transactionRepository;
    private readonly IRepository<WarehouseManagement.Inbound.InboundOrder, Guid> _inboundOrderRepository;
    private readonly IRepository<WarehouseManagement.Outbound.OutboundOrder, Guid> _outboundOrderRepository;
    private readonly IGuidGenerator _guidGenerator;

    public WarehouseSampleDataSeeder(
        IRepository<Warehouse, Guid> warehouseRepository,
        IRepository<WarehouseZone, Guid> zoneRepository,
        IRepository<WarehouseLocation, Guid> locationRepository,
        IRepository<WarehouseManagement.Products.Product, Guid> productRepository,
        IRepository<WarehouseManagement.Inventory.InventoryItem, Guid> inventoryRepository,
        IRepository<WarehouseManagement.Inventory.InventoryTransaction, Guid> transactionRepository,
        IRepository<WarehouseManagement.Inbound.InboundOrder, Guid> inboundOrderRepository,
        IRepository<WarehouseManagement.Outbound.OutboundOrder, Guid> outboundOrderRepository,
        IGuidGenerator guidGenerator)
    {
        _warehouseRepository = warehouseRepository;
        _zoneRepository = zoneRepository;
        _locationRepository = locationRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
        _inboundOrderRepository = inboundOrderRepository;
        _outboundOrderRepository = outboundOrderRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        Warehouse? warehouse1;
        Warehouse? warehouse2;

        if (await _warehouseRepository.GetCountAsync() == 0)
        {
            warehouse1 = new Warehouse(
                _guidGenerator.Create(),
                "WH-MUM-01",
                "Mumbai Central DC",
                WarehouseType.DistributionCenter,
                tenantId: context.TenantId,
                address: "Plot 12, Logistics Park, Bhiwandi",
                city: "Mumbai",
                state: "Maharashtra",
                postalCode: "421302",
                country: "India",
                contactPerson: "Rajesh Sharma",
                contactPhone: "+91-9876543210",
                contactEmail: "mumbai.dc@wms.com",
                totalAreaSqFt: 65000,
                isActive: true
            );

            warehouse2 = new Warehouse(
                _guidGenerator.Create(),
                "WH-DEL-01",
                "Delhi North Hub",
                WarehouseType.FulfillmentCenter,
                tenantId: context.TenantId,
                address: "Khasra 45, Kundli Industrial Area",
                city: "Delhi",
                state: "Delhi NCR",
                postalCode: "110040",
                country: "India",
                contactPerson: "Amit Verma",
                contactPhone: "+91-9812345678",
                contactEmail: "delhi.hub@wms.com",
                totalAreaSqFt: 85000,
                isActive: true
            );

            await _warehouseRepository.InsertAsync(warehouse1, autoSave: true);
            await _warehouseRepository.InsertAsync(warehouse2, autoSave: true);
        }
        else
        {
            warehouse1 = await _warehouseRepository.FirstOrDefaultAsync(x => x.Code == "WH-MUM-01");
            warehouse2 = await _warehouseRepository.FirstOrDefaultAsync(x => x.Code == "WH-DEL-01");
        }

        if (warehouse1 != null && await _zoneRepository.GetCountAsync() == 0)
        {
            var zoneAmbient = new WarehouseZone(
                _guidGenerator.Create(),
                warehouse1.Id,
                "ZONE-AMB",
                "Ambient Dry Storage",
                WarehouseZoneStorageType.Ambient,
                tenantId: context.TenantId,
                temperatureMinCelsius: 18,
                temperatureMaxCelsius: 25,
                isActive: true
            );

            var zoneCold = new WarehouseZone(
                _guidGenerator.Create(),
                warehouse1.Id,
                "ZONE-COLD",
                "Cold Storage (Pharma/Perishable)",
                WarehouseZoneStorageType.ColdStorage,
                tenantId: context.TenantId,
                temperatureMinCelsius: 2,
                temperatureMaxCelsius: 8,
                isActive: true
            );

            WarehouseZone? zoneBulkDelhi = null;
            if (warehouse2 != null)
            {
                zoneBulkDelhi = new WarehouseZone(
                    _guidGenerator.Create(),
                    warehouse2.Id,
                    "ZONE-BULK",
                    "Bulk Reserve Pallet Zone",
                    WarehouseZoneStorageType.BulkReserve,
                    tenantId: context.TenantId,
                    isActive: true
                );
                await _zoneRepository.InsertAsync(zoneBulkDelhi, autoSave: true);
            }

            await _zoneRepository.InsertAsync(zoneAmbient, autoSave: true);
            await _zoneRepository.InsertAsync(zoneCold, autoSave: true);

            // Seed Locations / Bins
            var loc1 = new WarehouseLocation(
                _guidGenerator.Create(),
                warehouse1.Id,
                zoneAmbient.Id,
                barcode: "MUM-A01-R01-S01-B01",
                aisle: "A01",
                rack: "R01",
                shelf: "S01",
                bin: "B01",
                locationType: WarehouseLocationType.PalletRack,
                tenantId: context.TenantId,
                status: WarehouseLocationStatus.Available,
                maxWeightKg: 1200,
                maxVolumeCm3: 1500000,
                lengthCm: 120,
                widthCm: 100,
                heightCm: 150,
                isPickFace: false,
                isActive: true
            );

            var loc2 = new WarehouseLocation(
                _guidGenerator.Create(),
                warehouse1.Id,
                zoneAmbient.Id,
                barcode: "MUM-A01-R01-S01-B02",
                aisle: "A01",
                rack: "R01",
                shelf: "S01",
                bin: "B02",
                locationType: WarehouseLocationType.PalletRack,
                tenantId: context.TenantId,
                status: WarehouseLocationStatus.Available,
                maxWeightKg: 1200,
                maxVolumeCm3: 1500000,
                lengthCm: 120,
                widthCm: 100,
                heightCm: 150,
                isPickFace: false,
                isActive: true
            );

            var loc3 = new WarehouseLocation(
                _guidGenerator.Create(),
                warehouse1.Id,
                zoneAmbient.Id,
                barcode: "MUM-A01-R01-S02-B01",
                aisle: "A01",
                rack: "R01",
                shelf: "S02",
                bin: "B01",
                locationType: WarehouseLocationType.Shelf,
                tenantId: context.TenantId,
                status: WarehouseLocationStatus.Available,
                maxWeightKg: 300,
                maxVolumeCm3: 300000,
                lengthCm: 60,
                widthCm: 50,
                heightCm: 40,
                isPickFace: true,
                isActive: true
            );

            var locCold = new WarehouseLocation(
                _guidGenerator.Create(),
                warehouse1.Id,
                zoneCold.Id,
                barcode: "MUM-C01-R01-S01-B01",
                aisle: "C01",
                rack: "R01",
                shelf: "S01",
                bin: "B01",
                locationType: WarehouseLocationType.PalletRack,
                tenantId: context.TenantId,
                status: WarehouseLocationStatus.Available,
                maxWeightKg: 1000,
                maxVolumeCm3: 1200000,
                lengthCm: 120,
                widthCm: 100,
                heightCm: 140,
                isPickFace: false,
                isActive: true
            );

            await _locationRepository.InsertAsync(loc1, autoSave: true);
            await _locationRepository.InsertAsync(loc2, autoSave: true);
            await _locationRepository.InsertAsync(loc3, autoSave: true);
            await _locationRepository.InsertAsync(locCold, autoSave: true);

            if (warehouse2 != null && zoneBulkDelhi != null)
            {
                var locDelhi = new WarehouseLocation(
                    _guidGenerator.Create(),
                    warehouse2.Id,
                    zoneBulkDelhi.Id,
                    barcode: "DEL-B01-R01-S01-B01",
                    aisle: "B01",
                    rack: "R01",
                    shelf: "S01",
                    bin: "B01",
                    locationType: WarehouseLocationType.FloorStorage,
                    tenantId: context.TenantId,
                    status: WarehouseLocationStatus.Available,
                    maxWeightKg: 2500,
                    maxVolumeCm3: 3500000,
                    lengthCm: 150,
                    widthCm: 150,
                    heightCm: 200,
                    isPickFace: false,
                    isActive: true
                );
                await _locationRepository.InsertAsync(locDelhi, autoSave: true);
            }
        }

    }
}
