using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Customers;
using WarehouseManagement.Inventory;
using WarehouseManagement.Outbound;
using WarehouseManagement.Products;
using WarehouseManagement.Suppliers;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Returns;

public class ReturnOrderAppService : ApplicationService, IReturnOrderAppService
{
    private readonly IRepository<ReturnOrder, Guid> _returnRepository;
    private readonly IRepository<ReturnOrderItem, Guid> _returnItemRepository;
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<WarehouseLocation, Guid> _locationRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<OutboundOrder, Guid> _outboundOrderRepository;
    private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
    private readonly IRepository<InventoryTransaction, Guid> _transactionRepository;

    public ReturnOrderAppService(
        IRepository<ReturnOrder, Guid> returnRepository,
        IRepository<ReturnOrderItem, Guid> returnItemRepository,
        IRepository<Warehouse, Guid> warehouseRepository,
        IRepository<WarehouseLocation, Guid> locationRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<OutboundOrder, Guid> outboundOrderRepository,
        IRepository<InventoryItem, Guid> inventoryRepository,
        IRepository<InventoryTransaction, Guid> transactionRepository)
    {
        _returnRepository = returnRepository;
        _returnItemRepository = returnItemRepository;
        _warehouseRepository = warehouseRepository;
        _locationRepository = locationRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _supplierRepository = supplierRepository;
        _outboundOrderRepository = outboundOrderRepository;
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<PagedResultDto<ReturnOrderDto>> GetListAsync(GetReturnOrderListDto input)
    {
        var queryable = await _returnRepository.WithDetailsAsync(x => x.Items);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim().ToLower();
            queryable = queryable.Where(x =>
                x.RmaNumber.ToLower().Contains(filter) ||
                (x.CustomerName != null && x.CustomerName.ToLower().Contains(filter)) ||
                (x.SupplierName != null && x.SupplierName.ToLower().Contains(filter)) ||
                (x.OutboundOrderNumber != null && x.OutboundOrderNumber.ToLower().Contains(filter)) ||
                (x.CarrierName != null && x.CarrierName.ToLower().Contains(filter)) ||
                (x.TrackingNumber != null && x.TrackingNumber.ToLower().Contains(filter)) ||
                (x.Notes != null && x.Notes.ToLower().Contains(filter)));
        }

        if (input.WarehouseId.HasValue)
        {
            queryable = queryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }

        if (input.CustomerId.HasValue)
        {
            queryable = queryable.Where(x => x.CustomerId == input.CustomerId.Value);
        }

        if (input.SupplierId.HasValue)
        {
            queryable = queryable.Where(x => x.SupplierId == input.SupplierId.Value);
        }

        if (input.ReturnType.HasValue)
        {
            queryable = queryable.Where(x => x.ReturnType == input.ReturnType.Value);
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        if (input.FromDate.HasValue)
        {
            queryable = queryable.Where(x => x.ReturnDate >= input.FromDate.Value);
        }

        if (input.ToDate.HasValue)
        {
            queryable = queryable.Where(x => x.ReturnDate <= input.ToDate.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime desc" : input.Sorting;
        queryable = queryable.OrderBy(sorting);

        var entities = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        var dtos = await MapToDtosAsync(entities);
        return new PagedResultDto<ReturnOrderDto>(totalCount, dtos);
    }

    public async Task<ReturnOrderDto> GetAsync(Guid id)
    {
        var queryable = await _returnRepository.WithDetailsAsync(x => x.Items);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (entity == null)
        {
            throw new UserFriendlyException($"Return order with Id '{id}' was not found.");
        }

        var dtos = await MapToDtosAsync(new List<ReturnOrder> { entity });
        return dtos.First();
    }

    public async Task<ReturnOrderDto> CreateAsync(CreateReturnOrderDto input)
    {
        var warehouse = await _warehouseRepository.FindAsync(input.WarehouseId);
        if (warehouse == null)
        {
            throw new UserFriendlyException("Selected receiving facility does not exist.");
        }

        if (input.Items == null || !input.Items.Any())
        {
            throw new UserFriendlyException("A return authorization order must contain at least one line item.");
        }

        // Generate RMA Number: RMA-yyyyMMdd-XXX
        var today = DateTime.UtcNow.Date;
        var prefix = $"RMA-{today:yyyyMMdd}-";
        var countToday = await _returnRepository.CountAsync(x => x.RmaNumber.StartsWith(prefix));
        var rmaNumber = $"{prefix}{(countToday + 1):D3}";

        string? customerName = input.CustomerName;
        if (input.CustomerId.HasValue && string.IsNullOrWhiteSpace(customerName))
        {
            var cust = await _customerRepository.FindAsync(input.CustomerId.Value);
            customerName = cust?.Name;
        }

        string? supplierName = input.SupplierName;
        if (input.SupplierId.HasValue && string.IsNullOrWhiteSpace(supplierName))
        {
            var supp = await _supplierRepository.FindAsync(input.SupplierId.Value);
            supplierName = supp?.Name;
        }

        var order = new ReturnOrder(
            GuidGenerator.Create(),
            rmaNumber,
            input.ReturnType,
            input.WarehouseId,
            input.CustomerId,
            customerName,
            input.SupplierId,
            supplierName,
            input.OutboundOrderId,
            input.OutboundOrderNumber,
            input.InboundOrderId,
            input.InboundOrderNumber,
            input.CarrierName,
            input.TrackingNumber,
            input.ReturnDate ?? DateTime.UtcNow,
            input.Notes,
            CurrentTenant.Id);

        var productIds = input.Items.Select(x => x.ProductId).Distinct().ToList();
        var products = await _productRepository.GetListAsync(x => productIds.Contains(x.Id));
        var productMap = products.ToDictionary(x => x.Id, x => x);

        foreach (var itemDto in input.Items)
        {
            if (!productMap.TryGetValue(itemDto.ProductId, out var product))
            {
                throw new UserFriendlyException($"Product with ID '{itemDto.ProductId}' was not found.");
            }

            var refundPrice = itemDto.UnitRefundPrice > 0 ? itemDto.UnitRefundPrice : product.UnitPrice;

            order.AddItem(
                product.Id,
                product.Sku,
                product.Name,
                itemDto.ExpectedQuantity,
                refundPrice,
                itemDto.ReturnReason,
                itemDto.LotBatchNumber,
                itemDto.SerialNumber);
        }

        await _returnRepository.InsertAsync(order, autoSave: true);
        return await GetAsync(order.Id);
    }

    public async Task<ReturnOrderDto> ApproveAsync(Guid id)
    {
        var order = await _returnRepository.GetAsync(id);
        order.Approve();
        await _returnRepository.UpdateAsync(order, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<ReturnOrderDto> MarkInTransitAsync(Guid id, string? carrierName, string? trackingNumber)
    {
        var order = await _returnRepository.GetAsync(id);
        order.MarkInTransit(carrierName, trackingNumber);
        await _returnRepository.UpdateAsync(order, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<ReturnOrderDto> ReceiveAsync(Guid id, ReceiveReturnDto input)
    {
        var queryable = await _returnRepository.WithDetailsAsync(x => x.Items);
        var order = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (order == null)
        {
            throw new UserFriendlyException($"Return order with Id '{id}' was not found.");
        }

        var itemMap = order.Items.ToDictionary(x => x.Id, x => x);

        foreach (var itemInput in input.Items)
        {
            if (itemMap.TryGetValue(itemInput.ItemId, out var lineItem))
            {
                lineItem.ReceivedQuantity = itemInput.ReceivedQuantity;
            }
        }

        order.MarkReceived(input.ReceivedDate, input.Notes);
        order.RecalculateTotals();
        await _returnRepository.UpdateAsync(order, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<ReturnOrderDto> InspectAndResolveAsync(Guid id, InspectReturnOrderDto input)
    {
        var queryable = await _returnRepository.WithDetailsAsync(x => x.Items);
        var order = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (order == null)
        {
            throw new UserFriendlyException($"Return order with Id '{id}' was not found.");
        }

        if (order.Status == ReturnOrderStatus.Completed)
        {
            throw new UserFriendlyException("This return order has already been finalized and closed.");
        }

        var itemMap = order.Items.ToDictionary(x => x.Id, x => x);

        foreach (var itemResolution in input.Items)
        {
            if (!itemMap.TryGetValue(itemResolution.ItemId, out var lineItem))
            {
                continue;
            }

            lineItem.ConditionGrade = itemResolution.ConditionGrade;
            lineItem.ResolutionAction = itemResolution.ResolutionAction;
            lineItem.RestockedQuantity = itemResolution.RestockedQuantity;
            lineItem.ScrappedQuantity = itemResolution.ScrappedQuantity;
            lineItem.DestinationLocationId = itemResolution.DestinationLocationId;
            lineItem.InspectionNotes = itemResolution.InspectionNotes?.Trim();

            // Automatic Physical Inventory Movements
            if (itemResolution.ResolutionAction == ItemResolutionAction.RestockToInventory && itemResolution.RestockedQuantity > 0)
            {
                if (!itemResolution.DestinationLocationId.HasValue)
                {
                    throw new UserFriendlyException($"Please specify a destination bin location to restock SKU '{lineItem.ProductSku}'.");
                }

                var location = await _locationRepository.FindAsync(itemResolution.DestinationLocationId.Value);
                if (location == null)
                {
                    throw new UserFriendlyException($"Selected destination location '{itemResolution.DestinationLocationId}' does not exist.");
                }

                // Update or create inventory on hand in destination location
                var invQuery = await _inventoryRepository.GetQueryableAsync();
                var inv = await AsyncExecuter.FirstOrDefaultAsync(invQuery.Where(x =>
                    x.WarehouseId == order.WarehouseId &&
                    x.ProductId == lineItem.ProductId &&
                    x.LocationId == itemResolution.DestinationLocationId.Value &&
                    x.LotBatchNumber == lineItem.LotBatchNumber &&
                    x.SerialNumber == lineItem.SerialNumber));

                if (inv != null)
                {
                    inv.QuantityOnHand += itemResolution.RestockedQuantity;
                    await _inventoryRepository.UpdateAsync(inv);
                }
                else
                {
                    var newInv = new InventoryItem(
                        GuidGenerator.Create(),
                        order.WarehouseId,
                        location.ZoneId,
                        itemResolution.DestinationLocationId.Value,
                        lineItem.ProductId,
                        itemResolution.RestockedQuantity,
                        CurrentTenant.Id,
                        lineItem.LotBatchNumber,
                        lineItem.SerialNumber);
                    await _inventoryRepository.InsertAsync(newInv);
                }

                // Log Inventory Transaction for Complete Audit Trail
                var tx = new InventoryTransaction(
                    GuidGenerator.Create(),
                    order.WarehouseId,
                    lineItem.ProductId,
                    itemResolution.RestockedQuantity,
                    InventoryTransactionType.CustomerReturn,
                    CurrentTenant.Id,
                    fromLocationId: null,
                    toLocationId: itemResolution.DestinationLocationId.Value,
                    lotBatchNumber: lineItem.LotBatchNumber,
                    serialNumber: lineItem.SerialNumber,
                    referenceNumber: order.RmaNumber,
                    notes: $"RMA Restock [{lineItem.ConditionGrade}]: {lineItem.InspectionNotes}",
                    transactionTime: DateTime.UtcNow);
                await _transactionRepository.InsertAsync(tx);
            }
            else if (itemResolution.ResolutionAction == ItemResolutionAction.ScrapAndWriteOff && itemResolution.ScrappedQuantity > 0)
            {
                var tx = new InventoryTransaction(
                    GuidGenerator.Create(),
                    order.WarehouseId,
                    lineItem.ProductId,
                    itemResolution.ScrappedQuantity,
                    InventoryTransactionType.DamageWriteOff,
                    CurrentTenant.Id,
                    fromLocationId: null,
                    toLocationId: null,
                    lotBatchNumber: lineItem.LotBatchNumber,
                    serialNumber: lineItem.SerialNumber,
                    referenceNumber: order.RmaNumber,
                    notes: $"RMA Scrap Write-off [{lineItem.ConditionGrade}]: {lineItem.InspectionNotes}",
                    transactionTime: DateTime.UtcNow);
                await _transactionRepository.InsertAsync(tx);
            }
        }

        order.Complete(input.InspectorName);
        await _returnRepository.UpdateAsync(order, autoSave: true);
        return await GetAsync(order.Id);
    }

    public async Task<ReturnOrderDto> RejectAsync(Guid id, string? reason)
    {
        var order = await _returnRepository.GetAsync(id);
        order.Reject(reason);
        await _returnRepository.UpdateAsync(order, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<ReturnOrderDto> CancelAsync(Guid id, string? reason)
    {
        var order = await _returnRepository.GetAsync(id);
        order.Cancel(reason);
        await _returnRepository.UpdateAsync(order, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<ReturnDashboardMetricsDto> GetDashboardMetricsAsync(Guid? warehouseId = null)
    {
        var queryable = await _returnRepository.GetQueryableAsync();
        if (warehouseId.HasValue)
        {
            queryable = queryable.Where(x => x.WarehouseId == warehouseId.Value);
        }

        var totalReturns = await AsyncExecuter.CountAsync(queryable);
        var pendingApproval = await AsyncExecuter.CountAsync(queryable.Where(x => x.Status == ReturnOrderStatus.Draft || x.Status == ReturnOrderStatus.Requested));
        var inTransit = await AsyncExecuter.CountAsync(queryable.Where(x => x.Status == ReturnOrderStatus.InTransit));
        var awaitingInspection = await AsyncExecuter.CountAsync(queryable.Where(x => x.Status == ReturnOrderStatus.Received || x.Status == ReturnOrderStatus.UnderInspection));
        var completed = await AsyncExecuter.CountAsync(queryable.Where(x => x.Status == ReturnOrderStatus.Completed));

        var itemsQueryable = await _returnItemRepository.GetQueryableAsync();
        if (warehouseId.HasValue)
        {
            var returnIds = await AsyncExecuter.ToListAsync(queryable.Select(x => x.Id));
            itemsQueryable = itemsQueryable.Where(x => returnIds.Contains(x.ReturnOrderId));
        }

        var totalRestocked = await AsyncExecuter.SumAsync(itemsQueryable, x => x.RestockedQuantity);
        var totalScrapped = await AsyncExecuter.SumAsync(itemsQueryable, x => x.ScrappedQuantity);
        var totalRefund = await AsyncExecuter.SumAsync(queryable, x => x.TotalRefundOrCreditAmount);

        var totalProcessedUnits = totalRestocked + totalScrapped;
        var recoveryRate = totalProcessedUnits > 0
            ? Math.Round((double)(totalRestocked / totalProcessedUnits) * 100.0, 1)
            : 100.0;

        return new ReturnDashboardMetricsDto
        {
            TotalReturns = totalReturns,
            PendingApprovalCount = pendingApproval,
            InTransitCount = inTransit,
            AwaitingInspectionCount = awaitingInspection,
            CompletedCount = completed,
            TotalRestockedUnits = totalRestocked,
            TotalScrappedUnits = totalScrapped,
            TotalRefundValue = totalRefund,
            RecoveryRatePercentage = recoveryRate
        };
    }

    private async Task<List<ReturnOrderDto>> MapToDtosAsync(List<ReturnOrder> entities)
    {
        if (!entities.Any())
        {
            return new List<ReturnOrderDto>();
        }

        var warehouseIds = entities.Select(x => x.WarehouseId).Distinct().ToList();
        var warehouses = await _warehouseRepository.GetListAsync(x => warehouseIds.Contains(x.Id));
        var warehouseMap = warehouses.ToDictionary(x => x.Id, x => x);

        var allLocationIds = entities.SelectMany(x => x.Items)
            .Where(x => x.DestinationLocationId.HasValue)
            .Select(x => x.DestinationLocationId!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, WarehouseLocation> locationMap = new();
        if (allLocationIds.Any())
        {
            var locations = await _locationRepository.GetListAsync(x => allLocationIds.Contains(x.Id));
            locationMap = locations.ToDictionary(x => x.Id, x => x);
        }

        return entities.Select(entity =>
        {
            warehouseMap.TryGetValue(entity.WarehouseId, out var wh);

            var dto = new ReturnOrderDto
            {
                Id = entity.Id,
                CreationTime = entity.CreationTime,
                CreatorId = entity.CreatorId,
                LastModificationTime = entity.LastModificationTime,
                LastModifierId = entity.LastModifierId,
                IsDeleted = entity.IsDeleted,
                RmaNumber = entity.RmaNumber,
                ReturnType = entity.ReturnType,
                Status = entity.Status,
                WarehouseId = entity.WarehouseId,
                WarehouseCode = wh?.Code,
                WarehouseName = wh?.Name,
                CustomerId = entity.CustomerId,
                CustomerName = entity.CustomerName,
                SupplierId = entity.SupplierId,
                SupplierName = entity.SupplierName,
                OutboundOrderId = entity.OutboundOrderId,
                OutboundOrderNumber = entity.OutboundOrderNumber,
                InboundOrderId = entity.InboundOrderId,
                InboundOrderNumber = entity.InboundOrderNumber,
                CarrierName = entity.CarrierName,
                TrackingNumber = entity.TrackingNumber,
                ReturnDate = entity.ReturnDate,
                ReceivedDate = entity.ReceivedDate,
                InspectedDate = entity.InspectedDate,
                InspectedBy = entity.InspectedBy,
                TotalQuantityExpected = entity.TotalQuantityExpected,
                TotalQuantityReceived = entity.TotalQuantityReceived,
                TotalQuantityRestocked = entity.TotalQuantityRestocked,
                TotalQuantityScrapped = entity.TotalQuantityScrapped,
                TotalRefundOrCreditAmount = entity.TotalRefundOrCreditAmount,
                Notes = entity.Notes,
                Items = entity.Items.Select(item =>
                {
                    locationMap.TryGetValue(item.DestinationLocationId ?? Guid.Empty, out var loc);

                    return new ReturnOrderItemDto
                    {
                        Id = item.Id,
                        CreationTime = item.CreationTime,
                        CreatorId = item.CreatorId,
                        ReturnOrderId = item.ReturnOrderId,
                        ProductId = item.ProductId,
                        ProductSku = item.ProductSku,
                        ProductName = item.ProductName,
                        ExpectedQuantity = item.ExpectedQuantity,
                        ReceivedQuantity = item.ReceivedQuantity,
                        RestockedQuantity = item.RestockedQuantity,
                        ScrappedQuantity = item.ScrappedQuantity,
                        UnitRefundPrice = item.UnitRefundPrice,
                        ReturnReason = item.ReturnReason,
                        ConditionGrade = item.ConditionGrade,
                        ResolutionAction = item.ResolutionAction,
                        DestinationLocationId = item.DestinationLocationId,
                        DestinationLocationBarcode = loc?.Barcode,
                        LotBatchNumber = item.LotBatchNumber,
                        SerialNumber = item.SerialNumber,
                        InspectionNotes = item.InspectionNotes
                    };
                }).ToList()
            };

            return dto;
        }).ToList();
    }
}
