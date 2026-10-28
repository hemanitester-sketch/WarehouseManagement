using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using WarehouseManagement.Warehouses;
using WarehouseManagement.Products;

namespace WarehouseManagement.Data;

public class WarehouseSampleDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<WarehouseZone, Guid> _zoneRepository;
    private readonly IRepository<WarehouseLocation, Guid> _locationRepository;
    private readonly IRepository<WarehouseManagement.Products.Product, Guid> _productRepository;
    private readonly IRepository<WarehouseManagement.Products.Category, Guid> _categoryRepository;
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
        IRepository<WarehouseManagement.Products.Category, Guid> categoryRepository,
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
        _categoryRepository = categoryRepository;
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

        if (await _categoryRepository.GetCountAsync() == 0)
        {
            var pharma = new Category(_guidGenerator.Create(), "PHARMACEUTICALS", "Pharmaceuticals", description: "Medicines, prescription drugs, tablets, and medical healthcare supplies.", tenantId: context.TenantId);
            var electronics = new Category(_guidGenerator.Create(), "ELECTRONICS", "Electronics", description: "Consumer electronics, computing devices, displays, and electronic accessories.", tenantId: context.TenantId);
            var apparel = new Category(_guidGenerator.Create(), "APPAREL", "Apparel & Fashion", description: "Clothing, footwear, sportswear, and fashion apparel.", tenantId: context.TenantId);
            var food = new Category(_guidGenerator.Create(), "FOOD-BEVERAGE", "Food & Beverage", description: "Packaged foods, beverages, canned items, and dry grocery supplies.", tenantId: context.TenantId);
            var chem = new Category(_guidGenerator.Create(), "CHEMICALS", "Chemicals", description: "Industrial chemicals, solvents, cleaning agents, and raw laboratory reagents.", tenantId: context.TenantId);
            var fmcg = new Category(_guidGenerator.Create(), "FMCG", "Fast Moving Consumer Goods (FMCG)", description: "Daily essentials, toiletries, household cleaning, and personal hygiene products.", tenantId: context.TenantId);
            var auto = new Category(_guidGenerator.Create(), "AUTOMOTIVE", "Automotive & Spare Parts", description: "Vehicle spare parts, lubricants, mechanical components, and auto accessories.", tenantId: context.TenantId);
            var general = new Category(_guidGenerator.Create(), "GENERAL", "General Merchandise", description: "General warehouse inventory, utility goods, packaging materials, and miscellaneous items.", tenantId: context.TenantId);

            await _categoryRepository.InsertAsync(pharma, autoSave: true);
            await _categoryRepository.InsertAsync(electronics, autoSave: true);
            await _categoryRepository.InsertAsync(apparel, autoSave: true);
            await _categoryRepository.InsertAsync(food, autoSave: true);
            await _categoryRepository.InsertAsync(chem, autoSave: true);
            await _categoryRepository.InsertAsync(fmcg, autoSave: true);
            await _categoryRepository.InsertAsync(auto, autoSave: true);
            await _categoryRepository.InsertAsync(general, autoSave: true);

            // Sample Subcategories
            var laptops = new Category(_guidGenerator.Create(), "ELEC-LAPTOPS", "Laptops & Computers", parentCategoryId: electronics.Id, description: "Notebooks, desktop computers, workstations, and servers.", tenantId: context.TenantId);
            var accessories = new Category(_guidGenerator.Create(), "ELEC-ACCESSORIES", "Peripherals & Accessories", parentCategoryId: electronics.Id, description: "Keyboards, mice, monitors, cables, and electronic peripherals.", tenantId: context.TenantId);
            var tablets = new Category(_guidGenerator.Create(), "PHARMA-TABLETS", "Tablets & Capsules", parentCategoryId: pharma.Id, description: "Oral solid dosage forms including pain relief, antibiotics, and vitamins.", tenantId: context.TenantId);
            var menswear = new Category(_guidGenerator.Create(), "APP-MENSWEAR", "Men's Wear", parentCategoryId: apparel.Id, description: "Men's shirts, trousers, jackets, and formal wear.", tenantId: context.TenantId);

            await _categoryRepository.InsertAsync(laptops, autoSave: true);
            await _categoryRepository.InsertAsync(accessories, autoSave: true);
            await _categoryRepository.InsertAsync(tablets, autoSave: true);
            await _categoryRepository.InsertAsync(menswear, autoSave: true);
        }
    }
}
