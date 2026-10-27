using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Products;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Inventory;

public class InventoryAppService : ApplicationService, IInventoryAppService
{
    private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
    private readonly IRepository<InventoryTransaction, Guid> _transactionRepository;
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<WarehouseZone, Guid> _zoneRepository;
    private readonly IRepository<WarehouseLocation, Guid> _locationRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public InventoryAppService(
        IRepository<InventoryItem, Guid> inventoryRepository,
        IRepository<InventoryTransaction, Guid> transactionRepository,
        IRepository<Warehouse, Guid> warehouseRepository,
        IRepository<WarehouseZone, Guid> zoneRepository,
        IRepository<WarehouseLocation, Guid> locationRepository,
        IRepository<Product, Guid> productRepository)
    {
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
        _warehouseRepository = warehouseRepository;
        _zoneRepository = zoneRepository;
        _locationRepository = locationRepository;
        _productRepository = productRepository;
    }

    public async Task<PagedResultDto<InventoryItemDto>> GetListAsync(GetInventoryListDto input)
    {
        var query = await _inventoryRepository.GetQueryableAsync();

        var now = DateTime.UtcNow;

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => (x.LotBatchNumber != null && x.LotBatchNumber.Contains(input.Filter!)) ||
                     (x.SerialNumber != null && x.SerialNumber.Contains(input.Filter!)) ||
                     (x.Lpn != null && x.Lpn.Contains(input.Filter!)))
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId!.Value)
            .WhereIf(input.ZoneId.HasValue, x => x.ZoneId == input.ZoneId!.Value)
            .WhereIf(input.LocationId.HasValue, x => x.LocationId == input.LocationId!.Value)
            .WhereIf(input.ProductId.HasValue, x => x.ProductId == input.ProductId!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.LotBatchNumber), x => x.LotBatchNumber == input.LotBatchNumber!)
            .WhereIf(!string.IsNullOrWhiteSpace(input.SerialNumber), x => x.SerialNumber == input.SerialNumber!)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Lpn), x => x.Lpn == input.Lpn!)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status!.Value)
            .WhereIf(input.ExpiredOnly == true, x => x.ExpiryDate != null && x.ExpiryDate < now);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var pagedQuery = query
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var items = await AsyncExecuter.ToListAsync(pagedQuery);

        var dtos = await MapToDtosAsync(items);

        return new PagedResultDto<InventoryItemDto>(totalCount, dtos);
    }

    public async Task<InventoryItemDto> GetAsync(Guid id)
    {
        var entity = await _inventoryRepository.GetAsync(id);
        var dtos = await MapToDtosAsync(new List<InventoryItem> { entity });
        return dtos.First();
    }

    public async Task<InventorySummaryDto> GetStockSummaryAsync(Guid warehouseId, Guid productId)
    {
        var warehouse = await _warehouseRepository.GetAsync(warehouseId);
        var product = await _productRepository.GetAsync(productId);

        var query = await _inventoryRepository.GetQueryableAsync();
        var stockItems = await AsyncExecuter.ToListAsync(query.Where(x => x.WarehouseId == warehouseId && x.ProductId == productId));

        return new InventorySummaryDto
        {
            WarehouseId = warehouse.Id,
            WarehouseCode = warehouse.Code,
            ProductId = product.Id,
            ProductSku = product.Sku,
            ProductName = product.Name,
            TotalOnHand = stockItems.Sum(x => x.QuantityOnHand),
            TotalAllocated = stockItems.Sum(x => x.QuantityAllocated),
            TotalAvailable = stockItems.Where(x => x.Status == InventoryStatus.Available).Sum(x => x.QuantityAvailable),
            TotalQuarantined = stockItems.Where(x => x.Status == InventoryStatus.Quarantined).Sum(x => x.QuantityOnHand),
            TotalDamaged = stockItems.Where(x => x.Status == InventoryStatus.Damaged).Sum(x => x.QuantityOnHand),
            LocationCount = stockItems.Select(x => x.LocationId).Distinct().Count()
        };
    }

    public async Task<InventoryItemDto> AdjustStockAsync(AdjustStockDto input)
    {
        if (input.QuantityDelta == 0)
        {
            throw new UserFriendlyException("Quantity delta cannot be zero.");
        }

        var location = await _locationRepository.GetAsync(input.LocationId);
        if (location.WarehouseId != input.WarehouseId)
        {
            throw new UserFriendlyException("Location does not belong to the specified warehouse.");
        }

        var normalizedLot = input.LotBatchNumber?.Trim();
        var normalizedSerial = input.SerialNumber?.Trim();
        var normalizedLpn = input.Lpn?.Trim();

        var query = await _inventoryRepository.GetQueryableAsync();
        var matchQuery = query.Where(x =>
            x.WarehouseId == input.WarehouseId &&
            x.LocationId == input.LocationId &&
            x.ProductId == input.ProductId &&
            x.LotBatchNumber == normalizedLot &&
            x.SerialNumber == normalizedSerial &&
            x.Lpn == normalizedLpn);

        var item = await AsyncExecuter.FirstOrDefaultAsync(matchQuery);

        if (item == null)
        {
            if (input.QuantityDelta < 0)
            {
                throw new UserFriendlyException("Cannot decrease stock for a non-existent inventory item.");
            }

            item = new InventoryItem(
                GuidGenerator.Create(),
                input.WarehouseId,
                location.ZoneId,
                input.LocationId,
                input.ProductId,
                input.QuantityDelta,
                CurrentTenant.Id,
                lotBatchNumber: normalizedLot,
                serialNumber: normalizedSerial,
                lpn: normalizedLpn,
                expiryDate: input.ExpiryDate,
                manufactureDate: input.ManufactureDate,
                status: input.Status
            );

            await _inventoryRepository.InsertAsync(item, autoSave: true);
        }
        else
        {
            item.AdjustQuantity(input.QuantityDelta);
            if (input.ExpiryDate.HasValue) item.ExpiryDate = input.ExpiryDate;
            if (input.ManufactureDate.HasValue) item.ManufactureDate = input.ManufactureDate;
            item.Status = input.Status;
            await _inventoryRepository.UpdateAsync(item, autoSave: true);
        }

        var transaction = new InventoryTransaction(
            GuidGenerator.Create(),
            input.WarehouseId,
            input.ProductId,
            Math.Abs(input.QuantityDelta),
            input.QuantityDelta > 0 ? InventoryTransactionType.AdjustmentIncrease : InventoryTransactionType.AdjustmentDecrease,
            CurrentTenant.Id,
            fromLocationId: input.QuantityDelta < 0 ? input.LocationId : null,
            toLocationId: input.QuantityDelta > 0 ? input.LocationId : null,
            lotBatchNumber: normalizedLot,
            serialNumber: normalizedSerial,
            lpn: normalizedLpn,
            referenceNumber: input.Reason,
            notes: input.Notes
        );

        await _transactionRepository.InsertAsync(transaction, autoSave: true);

        return await GetAsync(item.Id);
    }

    public async Task<InventoryItemDto> TransferStockAsync(TransferStockDto input)
    {
        if (input.Quantity <= 0)
        {
            throw new UserFriendlyException("Transfer quantity must be greater than zero.");
        }

        var sourceItem = await _inventoryRepository.GetAsync(input.SourceInventoryItemId);
        if (sourceItem.QuantityAvailable < input.Quantity)
        {
            throw new UserFriendlyException($"Insufficient available quantity to transfer. Available: {sourceItem.QuantityAvailable}, Requested: {input.Quantity}");
        }

        var destLocation = await _locationRepository.GetAsync(input.DestinationLocationId);
        if (destLocation.WarehouseId != input.DestinationWarehouseId)
        {
            throw new UserFriendlyException("Destination location does not belong to the target warehouse.");
        }

        // Deduct from source
        sourceItem.AdjustQuantity(-input.Quantity);
        await _inventoryRepository.UpdateAsync(sourceItem, autoSave: true);

        // Find or create in destination
        var query = await _inventoryRepository.GetQueryableAsync();
        var matchDestQuery = query.Where(x =>
            x.WarehouseId == input.DestinationWarehouseId &&
            x.LocationId == input.DestinationLocationId &&
            x.ProductId == sourceItem.ProductId &&
            x.LotBatchNumber == sourceItem.LotBatchNumber &&
            x.SerialNumber == sourceItem.SerialNumber &&
            x.Lpn == sourceItem.Lpn);

        var destItem = await AsyncExecuter.FirstOrDefaultAsync(matchDestQuery);

        if (destItem == null)
        {
            destItem = new InventoryItem(
                GuidGenerator.Create(),
                input.DestinationWarehouseId,
                destLocation.ZoneId,
                input.DestinationLocationId,
                sourceItem.ProductId,
                input.Quantity,
                CurrentTenant.Id,
                lotBatchNumber: sourceItem.LotBatchNumber,
                serialNumber: sourceItem.SerialNumber,
                lpn: sourceItem.Lpn,
                expiryDate: sourceItem.ExpiryDate,
                manufactureDate: sourceItem.ManufactureDate,
                status: sourceItem.Status
            );
            await _inventoryRepository.InsertAsync(destItem, autoSave: true);
        }
        else
        {
            destItem.AdjustQuantity(input.Quantity);
            await _inventoryRepository.UpdateAsync(destItem, autoSave: true);
        }

        // Record Audit Transaction
        var transaction = new InventoryTransaction(
            GuidGenerator.Create(),
            input.DestinationWarehouseId,
            sourceItem.ProductId,
            input.Quantity,
            InventoryTransactionType.BinTransfer,
            CurrentTenant.Id,
            fromLocationId: sourceItem.LocationId,
            toLocationId: input.DestinationLocationId,
            lotBatchNumber: sourceItem.LotBatchNumber,
            serialNumber: sourceItem.SerialNumber,
            lpn: sourceItem.Lpn,
            referenceNumber: input.ReferenceNumber ?? "BIN-TRANSFER",
            notes: input.Notes
        );

        await _transactionRepository.InsertAsync(transaction, autoSave: true);

        return await GetAsync(destItem.Id);
    }

    public async Task<PagedResultDto<InventoryTransactionDto>> GetTransactionsAsync(GetInventoryTransactionListDto input)
    {
        var query = await _transactionRepository.GetQueryableAsync();

        query = query
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId!.Value)
            .WhereIf(input.ProductId.HasValue, x => x.ProductId == input.ProductId!.Value)
            .WhereIf(input.TransactionType.HasValue, x => x.TransactionType == input.TransactionType!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ReferenceNumber), x => x.ReferenceNumber != null && x.ReferenceNumber.Contains(input.ReferenceNumber!));

        var totalCount = await AsyncExecuter.CountAsync(query);

        var pagedQuery = query
            .OrderByDescending(x => x.TransactionTime)
            .PageBy(input);

        var transactions = await AsyncExecuter.ToListAsync(pagedQuery);

        var dtos = transactions.Select(x => new InventoryTransactionDto
        {
            Id = x.Id,
            WarehouseId = x.WarehouseId,
            ProductId = x.ProductId,
            FromLocationId = x.FromLocationId,
            ToLocationId = x.ToLocationId,
            Quantity = x.Quantity,
            LotBatchNumber = x.LotBatchNumber,
            SerialNumber = x.SerialNumber,
            Lpn = x.Lpn,
            TransactionType = x.TransactionType,
            ReferenceNumber = x.ReferenceNumber,
            Notes = x.Notes,
            TransactionTime = x.TransactionTime
        }).ToList();

        return new PagedResultDto<InventoryTransactionDto>(totalCount, dtos);
    }

    private async Task<List<InventoryItemDto>> MapToDtosAsync(List<InventoryItem> items)
    {
        if (!items.Any()) return new List<InventoryItemDto>();

        var warehouseIds = items.Select(x => x.WarehouseId).Distinct().ToList();
        var zoneIds = items.Select(x => x.ZoneId).Distinct().ToList();
        var locationIds = items.Select(x => x.LocationId).Distinct().ToList();
        var productIds = items.Select(x => x.ProductId).Distinct().ToList();

        var warehouses = (await _warehouseRepository.GetListAsync(x => warehouseIds.Contains(x.Id))).ToDictionary(x => x.Id);
        var zones = (await _zoneRepository.GetListAsync(x => zoneIds.Contains(x.Id))).ToDictionary(x => x.Id);
        var locations = (await _locationRepository.GetListAsync(x => locationIds.Contains(x.Id))).ToDictionary(x => x.Id);
        var products = (await _productRepository.GetListAsync(x => productIds.Contains(x.Id))).ToDictionary(x => x.Id);

        return items.Select(item =>
        {
            warehouses.TryGetValue(item.WarehouseId, out var wh);
            zones.TryGetValue(item.ZoneId, out var zn);
            locations.TryGetValue(item.LocationId, out var loc);
            products.TryGetValue(item.ProductId, out var prod);

            return new InventoryItemDto
            {
                Id = item.Id,
                WarehouseId = item.WarehouseId,
                WarehouseCode = wh?.Code,
                WarehouseName = wh?.Name,
                ZoneId = item.ZoneId,
                ZoneCode = zn?.Code,
                LocationId = item.LocationId,
                LocationBarcode = loc?.Barcode,
                LocationCoordinates = loc != null ? $"{loc.Aisle}-{loc.Rack}-{loc.Shelf}-{loc.Bin}" : null,
                ProductId = item.ProductId,
                ProductSku = prod?.Sku,
                ProductName = prod?.Name,
                QuantityOnHand = item.QuantityOnHand,
                QuantityAllocated = item.QuantityAllocated,
                QuantityAvailable = item.QuantityAvailable,
                LotBatchNumber = item.LotBatchNumber,
                SerialNumber = item.SerialNumber,
                Lpn = item.Lpn,
                ExpiryDate = item.ExpiryDate,
                ManufactureDate = item.ManufactureDate,
                Status = item.Status,
                ReceivedDate = item.ReceivedDate,
                CreationTime = item.CreationTime,
                CreatorId = item.CreatorId,
                LastModificationTime = item.LastModificationTime,
                LastModifierId = item.LastModifierId
            };
        }).ToList();
    }
}
