using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Inventory;
using WarehouseManagement.Products;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Outbound;

public class OutboundOrderAppService : ApplicationService, IOutboundOrderAppService
{
    private readonly IRepository<OutboundOrder, Guid> _orderRepository;
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
    private readonly IRepository<InventoryTransaction, Guid> _transactionRepository;

    public OutboundOrderAppService(
        IRepository<OutboundOrder, Guid> orderRepository,
        IRepository<Warehouse, Guid> warehouseRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<InventoryItem, Guid> inventoryRepository,
        IRepository<InventoryTransaction, Guid> transactionRepository)
    {
        _orderRepository = orderRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<PagedResultDto<OutboundOrderDto>> GetListAsync(GetOutboundOrderListDto input)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.OrderNumber.Contains(input.Filter!) || x.CustomerName.Contains(input.Filter!))
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId!.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status!.Value)
            .WhereIf(input.Priority.HasValue, x => x.Priority == input.Priority!.Value)
            .WhereIf(input.OrderDateFrom.HasValue, x => x.OrderDate >= input.OrderDateFrom!.Value)
            .WhereIf(input.OrderDateTo.HasValue, x => x.OrderDate <= input.OrderDateTo!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var pagedQuery = query
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var orders = await AsyncExecuter.ToListAsync(pagedQuery);

        var dtos = await MapToDtosAsync(orders);

        return new PagedResultDto<OutboundOrderDto>(totalCount, dtos);
    }

    public async Task<OutboundOrderDto> GetAsync(Guid id)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));

        if (order == null)
        {
            throw new EntityNotFoundException(typeof(OutboundOrder), id);
        }

        var dtos = await MapToDtosAsync(new List<OutboundOrder> { order });
        return dtos.First();
    }

    public async Task<OutboundOrderDto> CreateAsync(CreateUpdateOutboundOrderDto input)
    {
        var existing = await _orderRepository.FirstOrDefaultAsync(x => x.OrderNumber == input.OrderNumber.Trim().ToUpperInvariant());
        if (existing != null)
        {
            throw new UserFriendlyException($"Outbound order number '{input.OrderNumber}' already exists.");
        }

        var order = new OutboundOrder(
            GuidGenerator.Create(),
            input.OrderNumber,
            input.WarehouseId,
            input.CustomerName,
            input.OrderDate,
            CurrentTenant.Id,
            input.ShippingAddress,
            input.City,
            input.State,
            input.PostalCode,
            input.Country,
            input.Priority,
            OutboundOrderStatus.Draft,
            input.RequiredDeliveryDate,
            input.Notes
        );

        if (input.Lines != null)
        {
            foreach (var lineDto in input.Lines)
            {
                order.AddLine(GuidGenerator.Create(), lineDto.ProductId, lineDto.OrderedQuantity, lineDto.UnitPrice);
            }
        }

        await _orderRepository.InsertAsync(order, autoSave: true);

        return await GetAsync(order.Id);
    }

    public async Task<OutboundOrderDto> UpdateAsync(Guid id, CreateUpdateOutboundOrderDto input)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));

        if (order == null)
        {
            throw new EntityNotFoundException(typeof(OutboundOrder), id);
        }

        if (order.Status == OutboundOrderStatus.Shipped || order.Status == OutboundOrderStatus.Picked)
        {
            throw new UserFriendlyException("Cannot edit an order that is already picked or shipped.");
        }

        order.SetOrderNumber(input.OrderNumber);
        order.WarehouseId = input.WarehouseId;
        order.SetCustomerName(input.CustomerName);
        order.ShippingAddress = input.ShippingAddress?.Trim();
        order.City = input.City?.Trim();
        order.State = input.State?.Trim();
        order.PostalCode = input.PostalCode?.Trim();
        order.Country = input.Country?.Trim();
        order.Priority = input.Priority;
        order.OrderDate = input.OrderDate;
        order.RequiredDeliveryDate = input.RequiredDeliveryDate;
        order.Notes = input.Notes?.Trim();

        await _orderRepository.UpdateAsync(order, autoSave: true);

        return await GetAsync(order.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));

        if (order == null)
        {
            return;
        }

        if (order.Status != OutboundOrderStatus.Draft && order.Status != OutboundOrderStatus.Cancelled)
        {
            throw new UserFriendlyException("Only Draft or Cancelled orders can be deleted.");
        }

        await _orderRepository.DeleteAsync(order, autoSave: true);
    }

    public async Task<OutboundOrderDto> AllocateStockAsync(AllocateOrderStockDto input)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == input.OutboundOrderId));

        if (order == null)
        {
            throw new EntityNotFoundException(typeof(OutboundOrder), input.OutboundOrderId);
        }

        if (order.Status != OutboundOrderStatus.Draft && 
            order.Status != OutboundOrderStatus.Allocated && 
            order.Status != OutboundOrderStatus.Picking)
        {
            throw new UserFriendlyException($"Order cannot be allocated from status '{order.Status}'.");
        }

        var invQuery = await _inventoryRepository.GetQueryableAsync();

        foreach (var line in order.Lines)
        {
            var needed = line.OrderedQuantity - line.AllocatedQuantity;
            if (needed <= 0) continue;

            // FEFO sorting: items with earliest expiry date first
            var availableItems = await AsyncExecuter.ToListAsync(invQuery
                .Where(x => x.WarehouseId == order.WarehouseId &&
                            x.ProductId == line.ProductId &&
                            x.Status == InventoryStatus.Available &&
                            (x.QuantityOnHand - x.QuantityAllocated) > 0)
                .OrderBy(x => x.ExpiryDate == null)
                .ThenBy(x => x.ExpiryDate)
                .ThenBy(x => x.CreationTime));

            foreach (var stockItem in availableItems)
            {
                if (needed <= 0) break;

                var allocatable = Math.Min(needed, stockItem.QuantityAvailable);
                if (allocatable > 0)
                {
                    stockItem.AllocateQuantity(allocatable);
                    line.AddAllocatedQuantity(allocatable);
                    needed -= allocatable;
                    await _inventoryRepository.UpdateAsync(stockItem, autoSave: true);
                }
            }
        }

        if (order.Status == OutboundOrderStatus.Picking)
        {
            // Retain Picking status if lines have already been partially picked
        }
        else if (order.Lines.All(x => x.AllocatedQuantity == 0))
        {
            order.Status = OutboundOrderStatus.Draft;
        }
        else
        {
            order.Status = OutboundOrderStatus.Allocated;
        }

        await _orderRepository.UpdateAsync(order, autoSave: true);

        return await GetAsync(order.Id);
    }

    public async Task<OutboundOrderDto> PickGoodsAsync(PickOrderGoodsDto input)
    {
        if (input.PickedQuantity <= 0)
        {
            throw new UserFriendlyException("Picked quantity must be greater than zero.");
        }

        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == input.OutboundOrderId));

        if (order == null)
        {
            throw new EntityNotFoundException(typeof(OutboundOrder), input.OutboundOrderId);
        }

        var line = order.Lines.FirstOrDefault(x => x.Id == input.OutboundOrderLineId);
        if (line == null)
        {
            throw new UserFriendlyException("Order line not found.");
        }

        var remainingAllocated = line.AllocatedQuantity - line.PickedQuantity;
        if (input.PickedQuantity > remainingAllocated)
        {
            throw new UserFriendlyException($"Requested pick quantity ({input.PickedQuantity}) exceeds remaining allocated quantity ({remainingAllocated}) for this line.");
        }

        var stockItem = await _inventoryRepository.GetAsync(input.SourceInventoryItemId);
        if (stockItem.QuantityOnHand < input.PickedQuantity)
        {
            throw new UserFriendlyException($"Insufficient stock on hand in source bin. Available on hand: {stockItem.QuantityOnHand}, Requested pick: {input.PickedQuantity}");
        }

        // 1. Deduct stock from bin
        stockItem.AdjustQuantity(-input.PickedQuantity);
        stockItem.ReleaseAllocatedQuantity(input.PickedQuantity);
        await _inventoryRepository.UpdateAsync(stockItem, autoSave: true);

        // 2. Update line picked quantity and status
        order.PickItem(line.Id, input.PickedQuantity);
        await _orderRepository.UpdateAsync(order, autoSave: true);

        // 3. Record Audit Transaction
        var transaction = new InventoryTransaction(
            GuidGenerator.Create(),
            order.WarehouseId,
            line.ProductId,
            input.PickedQuantity,
            InventoryTransactionType.OutboundPick,
            CurrentTenant.Id,
            fromLocationId: stockItem.LocationId,
            toLocationId: null,
            lotBatchNumber: stockItem.LotBatchNumber,
            serialNumber: stockItem.SerialNumber,
            lpn: stockItem.Lpn,
            referenceNumber: order.OrderNumber,
            notes: input.Notes ?? $"Picked for Sales Order {order.OrderNumber}"
        );

        await _transactionRepository.InsertAsync(transaction, autoSave: true);

        return await GetAsync(order.Id);
    }

    public async Task<OutboundOrderDto> ShipOrderAsync(ShipOrderDto input)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == input.OutboundOrderId));

        if (order == null)
        {
            throw new EntityNotFoundException(typeof(OutboundOrder), input.OutboundOrderId);
        }

        order.Ship(input.CarrierName, input.TrackingNumber);
        await _orderRepository.UpdateAsync(order, autoSave: true);

        return await GetAsync(order.Id);
    }

    private async Task<List<OutboundOrderDto>> MapToDtosAsync(List<OutboundOrder> orders)
    {
        if (!orders.Any()) return new List<OutboundOrderDto>();

        var warehouseIds = orders.Select(x => x.WarehouseId).Distinct().ToList();
        var productIds = orders.SelectMany(x => x.Lines).Select(l => l.ProductId).Distinct().ToList();

        var warehouses = (await _warehouseRepository.GetListAsync(x => warehouseIds.Contains(x.Id))).ToDictionary(x => x.Id);
        var products = (await _productRepository.GetListAsync(x => productIds.Contains(x.Id))).ToDictionary(x => x.Id);

        return orders.Select(order =>
        {
            warehouses.TryGetValue(order.WarehouseId, out var wh);

            var lineDtos = order.Lines.Select(line =>
            {
                products.TryGetValue(line.ProductId, out var prod);

                return new OutboundOrderLineDto
                {
                    Id = line.Id,
                    OutboundOrderId = line.OutboundOrderId,
                    ProductId = line.ProductId,
                    ProductSku = prod?.Sku,
                    ProductName = prod?.Name,
                    OrderedQuantity = line.OrderedQuantity,
                    AllocatedQuantity = line.AllocatedQuantity,
                    PickedQuantity = line.PickedQuantity,
                    UnitPrice = line.UnitPrice,
                    IsCompleted = line.IsCompleted
                };
            }).ToList();

            return new OutboundOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                WarehouseId = order.WarehouseId,
                WarehouseCode = wh?.Code,
                WarehouseName = wh?.Name,
                CustomerName = order.CustomerName,
                ShippingAddress = order.ShippingAddress,
                City = order.City,
                State = order.State,
                PostalCode = order.PostalCode,
                Country = order.Country,
                Priority = order.Priority,
                Status = order.Status,
                OrderDate = order.OrderDate,
                RequiredDeliveryDate = order.RequiredDeliveryDate,
                ShippedDate = order.ShippedDate,
                CarrierName = order.CarrierName,
                TrackingNumber = order.TrackingNumber,
                Notes = order.Notes,
                TotalLines = order.TotalLines,
                TotalOrderedQuantity = order.TotalOrderedQuantity,
                TotalAllocatedQuantity = order.TotalAllocatedQuantity,
                TotalPickedQuantity = order.TotalPickedQuantity,
                Lines = lineDtos,
                CreationTime = order.CreationTime,
                CreatorId = order.CreatorId,
                LastModificationTime = order.LastModificationTime,
                LastModifierId = order.LastModifierId
            };
        }).ToList();
    }
}
