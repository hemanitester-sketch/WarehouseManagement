using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Inventory;
using WarehouseManagement.Products;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Transfers;

public class StockTransferAppService : ApplicationService, IStockTransferAppService
{
    private readonly IRepository<StockTransfer, Guid> _transferRepository;
    private readonly IRepository<StockTransferItem, Guid> _transferItemRepository;
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<WarehouseLocation, Guid> _locationRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
    private readonly IRepository<InventoryTransaction, Guid> _transactionRepository;

    public StockTransferAppService(
        IRepository<StockTransfer, Guid> transferRepository,
        IRepository<StockTransferItem, Guid> transferItemRepository,
        IRepository<Warehouse, Guid> warehouseRepository,
        IRepository<WarehouseLocation, Guid> locationRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<InventoryItem, Guid> inventoryRepository,
        IRepository<InventoryTransaction, Guid> transactionRepository)
    {
        _transferRepository = transferRepository;
        _transferItemRepository = transferItemRepository;
        _warehouseRepository = warehouseRepository;
        _locationRepository = locationRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<PagedResultDto<StockTransferDto>> GetListAsync(GetStockTransferListDto input)
    {
        var queryable = await _transferRepository.WithDetailsAsync(x => x.Items);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim().ToLower();
            queryable = queryable.Where(x =>
                x.TransferNumber.ToLower().Contains(filter) ||
                (x.CarrierName != null && x.CarrierName.ToLower().Contains(filter)) ||
                (x.VehicleNumber != null && x.VehicleNumber.ToLower().Contains(filter)) ||
                (x.DriverName != null && x.DriverName.ToLower().Contains(filter)) ||
                (x.Notes != null && x.Notes.ToLower().Contains(filter)));
        }

        if (input.TransferType.HasValue)
        {
            queryable = queryable.Where(x => x.TransferType == input.TransferType.Value);
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        if (input.SourceWarehouseId.HasValue)
        {
            queryable = queryable.Where(x => x.SourceWarehouseId == input.SourceWarehouseId.Value);
        }

        if (input.DestinationWarehouseId.HasValue)
        {
            queryable = queryable.Where(x => x.DestinationWarehouseId == input.DestinationWarehouseId.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime desc" : input.Sorting;
        queryable = queryable.OrderBy(sorting);

        var entities = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        var dtos = await MapToDtosAsync(entities);

        return new PagedResultDto<StockTransferDto>(totalCount, dtos);
    }

    public async Task<StockTransferDto> GetAsync(Guid id)
    {
        var queryable = await _transferRepository.WithDetailsAsync(x => x.Items);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (entity == null)
        {
            throw new UserFriendlyException($"Stock transfer with Id '{id}' not found.");
        }

        var dtos = await MapToDtosAsync(new List<StockTransfer> { entity });
        return dtos.First();
    }

    public async Task<StockTransferDto> CreateAsync(CreateUpdateStockTransferDto input)
    {
        if (input.SourceWarehouseId == input.DestinationWarehouseId && input.TransferType == TransferType.InterWarehouse)
        {
            throw new UserFriendlyException("Inter-warehouse transfer cannot have the same source and destination facility.");
        }

        if (input.Items == null || !input.Items.Any())
        {
            throw new UserFriendlyException("A transfer order must contain at least one item.");
        }

        // Auto-generate Transfer Number: TR-YYYY-XXXX
        var year = DateTime.UtcNow.Year;
        var count = await _transferRepository.GetCountAsync();
        var transferNumber = $"TR-{year}-{(count + 1):D4}";

        var transfer = new StockTransfer(
            Guid.NewGuid(),
            transferNumber,
            input.TransferType,
            input.SourceWarehouseId,
            input.DestinationWarehouseId,
            CurrentTenant.Id,
            input.SourceLocationId,
            input.DestinationLocationId,
            input.CarrierName,
            input.TrackingNumber,
            input.VehicleNumber,
            input.DriverName,
            input.DriverPhone,
            input.TransferDate ?? DateTime.UtcNow,
            input.Notes
        );

        foreach (var itemDto in input.Items)
        {
            transfer.AddItem(
                itemDto.ProductId,
                itemDto.QuantityRequested,
                itemDto.SourceLocationId ?? input.SourceLocationId,
                itemDto.DestinationLocationId ?? input.DestinationLocationId,
                itemDto.QuantityRequested,
                itemDto.LotBatchNumber,
                itemDto.SerialNumber,
                itemDto.Lpn
            );
        }

        await _transferRepository.InsertAsync(transfer, autoSave: true);

        return await GetAsync(transfer.Id);
    }

    public async Task<StockTransferDto> ApproveAsync(Guid id)
    {
        var transfer = await _transferRepository.GetAsync(id);
        transfer.Approve();
        await _transferRepository.UpdateAsync(transfer, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<StockTransferDto> DispatchAsync(Guid id, DispatchTransferDto input)
    {
        var transfer = await _transferRepository.GetAsync(id);
        transfer.Dispatch(
            input.CarrierName,
            input.TrackingNumber,
            input.VehicleNumber,
            input.DriverName,
            input.DriverPhone
        );

        await _transferRepository.UpdateAsync(transfer, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<StockTransferDto> CompleteAsync(Guid id)
    {
        var queryable = await _transferRepository.WithDetailsAsync(x => x.Items);
        var transfer = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (transfer == null)
        {
            throw new UserFriendlyException($"Stock transfer with Id '{id}' not found.");
        }

        if (transfer.Status == TransferStatus.Completed)
        {
            throw new UserFriendlyException("Transfer order is already completed.");
        }

        // Execute physical inventory movements for each item
        foreach (var item in transfer.Items)
        {
            var srcLocId = item.SourceLocationId ?? transfer.SourceLocationId;
            var destLocId = item.DestinationLocationId ?? transfer.DestinationLocationId;

            // 1. Deduct from Source Warehouse & Location
            if (srcLocId.HasValue)
            {
                var srcInventoryQuery = await _inventoryRepository.GetQueryableAsync();
                var srcItem = await AsyncExecuter.FirstOrDefaultAsync(srcInventoryQuery.Where(x =>
                    x.WarehouseId == transfer.SourceWarehouseId &&
                    x.LocationId == srcLocId.Value &&
                    x.ProductId == item.ProductId));

                if (srcItem != null)
                {
                    srcItem.AdjustQuantity(-item.QuantityTransferred);
                    await _inventoryRepository.UpdateAsync(srcItem, autoSave: true);
                }
            }

            // 2. Add / Update in Destination Warehouse & Location
            if (destLocId.HasValue)
            {
                var destLocation = await _locationRepository.FindAsync(destLocId.Value);
                var destZoneId = destLocation?.ZoneId ?? Guid.Empty;

                var destInventoryQuery = await _inventoryRepository.GetQueryableAsync();
                var destItem = await AsyncExecuter.FirstOrDefaultAsync(destInventoryQuery.Where(x =>
                    x.WarehouseId == transfer.DestinationWarehouseId &&
                    x.LocationId == destLocId.Value &&
                    x.ProductId == item.ProductId));

                if (destItem == null)
                {
                    destItem = new InventoryItem(
                        Guid.NewGuid(),
                        transfer.DestinationWarehouseId,
                        destZoneId,
                        destLocId.Value,
                        item.ProductId,
                        item.QuantityTransferred,
                        CurrentTenant.Id,
                        item.LotBatchNumber,
                        item.SerialNumber,
                        item.Lpn,
                        status: transfer.TransferType == TransferType.Quarantine ? InventoryStatus.Quarantined : InventoryStatus.Available,
                        receivedDate: DateTime.UtcNow
                    );
                    await _inventoryRepository.InsertAsync(destItem, autoSave: true);
                }
                else
                {
                    destItem.AdjustQuantity(item.QuantityTransferred);
                    if (transfer.TransferType == TransferType.Quarantine)
                    {
                        destItem.SetStatus(InventoryStatus.Quarantined);
                    }
                    await _inventoryRepository.UpdateAsync(destItem, autoSave: true);
                }
            }

            // 3. Record Audit Log Transaction
            var auditTx = new InventoryTransaction(
                Guid.NewGuid(),
                transfer.SourceWarehouseId,
                item.ProductId,
                item.QuantityTransferred,
                InventoryTransactionType.BinTransfer,
                CurrentTenant.Id,
                fromLocationId: srcLocId,
                toLocationId: destLocId,
                lotBatchNumber: item.LotBatchNumber,
                serialNumber: item.SerialNumber,
                lpn: item.Lpn,
                referenceNumber: transfer.TransferNumber,
                notes: $"{transfer.TransferType} movement ({transfer.TransferNumber}) completed."
            );

            await _transactionRepository.InsertAsync(auditTx, autoSave: true);
        }

        transfer.Complete();
        await _transferRepository.UpdateAsync(transfer, autoSave: true);

        return await GetAsync(id);
    }

    public async Task<StockTransferDto> CancelAsync(Guid id, string? reason = null)
    {
        var transfer = await _transferRepository.GetAsync(id);
        transfer.Cancel(reason);
        await _transferRepository.UpdateAsync(transfer, autoSave: true);
        return await GetAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var transfer = await _transferRepository.GetAsync(id);
        if (transfer.Status != TransferStatus.Draft && transfer.Status != TransferStatus.Cancelled)
        {
            throw new UserFriendlyException("Only Draft or Cancelled transfer orders can be deleted.");
        }

        await _transferRepository.DeleteAsync(transfer);
    }

    private async Task<List<StockTransferDto>> MapToDtosAsync(List<StockTransfer> transfers)
    {
        if (!transfers.Any())
        {
            return new List<StockTransferDto>();
        }

        var warehouseIds = transfers.Select(x => x.SourceWarehouseId)
            .Concat(transfers.Select(x => x.DestinationWarehouseId))
            .Distinct()
            .ToList();

        var warehouses = await _warehouseRepository.GetListAsync(x => warehouseIds.Contains(x.Id));
        var warehouseMap = warehouses.ToDictionary(x => x.Id, x => x);

        var productIds = transfers.SelectMany(x => x.Items).Select(x => x.ProductId).Distinct().ToList();
        var products = await _productRepository.GetListAsync(x => productIds.Contains(x.Id));
        var productMap = products.ToDictionary(x => x.Id, x => x);

        var locationIds = transfers.SelectMany(x => x.Items)
            .SelectMany(x => new[] { x.SourceLocationId, x.DestinationLocationId })
            .Concat(transfers.SelectMany(x => new[] { x.SourceLocationId, x.DestinationLocationId }))
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        var locations = await _locationRepository.GetListAsync(x => locationIds.Contains(x.Id));
        var locationMap = locations.ToDictionary(x => x.Id, x => x);

        var dtos = new List<StockTransferDto>();

        foreach (var t in transfers)
        {
            var srcWh = warehouseMap.GetValueOrDefault(t.SourceWarehouseId);
            var destWh = warehouseMap.GetValueOrDefault(t.DestinationWarehouseId);

            var srcLoc = t.SourceLocationId.HasValue ? locationMap.GetValueOrDefault(t.SourceLocationId.Value) : null;
            var destLoc = t.DestinationLocationId.HasValue ? locationMap.GetValueOrDefault(t.DestinationLocationId.Value) : null;

            var dto = new StockTransferDto
            {
                Id = t.Id,
                CreationTime = t.CreationTime,
                TransferNumber = t.TransferNumber,
                TransferType = t.TransferType,
                TransferTypeLabel = GetTransferTypeLabel(t.TransferType),
                Status = t.Status,
                StatusLabel = t.Status.ToString(),
                SourceWarehouseId = t.SourceWarehouseId,
                SourceWarehouseCode = srcWh?.Code,
                SourceWarehouseName = srcWh?.Name,
                DestinationWarehouseId = t.DestinationWarehouseId,
                DestinationWarehouseCode = destWh?.Code,
                DestinationWarehouseName = destWh?.Name,
                SourceLocationId = t.SourceLocationId,
                SourceLocationBarcode = srcLoc?.Barcode,
                DestinationLocationId = t.DestinationLocationId,
                DestinationLocationBarcode = destLoc?.Barcode,
                CarrierName = t.CarrierName,
                TrackingNumber = t.TrackingNumber,
                VehicleNumber = t.VehicleNumber,
                DriverName = t.DriverName,
                DriverPhone = t.DriverPhone,
                TransferDate = t.TransferDate,
                DispatchedDate = t.DispatchedDate,
                ReceivedDate = t.ReceivedDate,
                Notes = t.Notes,
                TotalItemsCount = t.Items?.Count ?? 0,
                TotalQuantity = t.Items?.Sum(x => x.QuantityTransferred) ?? 0,
                Items = t.Items?.Select(item =>
                {
                    var p = productMap.GetValueOrDefault(item.ProductId);
                    var itemSrcLoc = item.SourceLocationId.HasValue ? locationMap.GetValueOrDefault(item.SourceLocationId.Value) : null;
                    var itemDestLoc = item.DestinationLocationId.HasValue ? locationMap.GetValueOrDefault(item.DestinationLocationId.Value) : null;

                    return new StockTransferItemDto
                    {
                        Id = item.Id,
                        StockTransferId = item.StockTransferId,
                        ProductId = item.ProductId,
                        ProductSku = p?.Sku,
                        ProductName = p?.Name,
                        SourceLocationId = item.SourceLocationId,
                        SourceLocationBarcode = itemSrcLoc?.Barcode,
                        DestinationLocationId = item.DestinationLocationId,
                        DestinationLocationBarcode = itemDestLoc?.Barcode,
                        QuantityRequested = item.QuantityRequested,
                        QuantityTransferred = item.QuantityTransferred,
                        LotBatchNumber = item.LotBatchNumber,
                        SerialNumber = item.SerialNumber,
                        Lpn = item.Lpn
                    };
                }).ToList() ?? new List<StockTransferItemDto>()
            };

            dtos.Add(dto);
        }

        return dtos;
    }

    private static string GetTransferTypeLabel(TransferType type)
    {
        return type switch
        {
            TransferType.InterWarehouse => "Inter-Warehouse",
            TransferType.BinToBin => "Bin-to-Bin Relocation",
            TransferType.Replenishment => "Fulfillment Replenishment",
            TransferType.Quarantine => "Quarantine / Inspection",
            _ => type.ToString()
        };
    }
}
