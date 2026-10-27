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

namespace WarehouseManagement.Inbound;

public class InboundOrderAppService : ApplicationService, IInboundOrderAppService
{
    private readonly IRepository<InboundOrder, Guid> _orderRepository;
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<WarehouseLocation, Guid> _locationRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
    private readonly IRepository<InventoryTransaction, Guid> _transactionRepository;

    public InboundOrderAppService(
        IRepository<InboundOrder, Guid> orderRepository,
        IRepository<Warehouse, Guid> warehouseRepository,
        IRepository<WarehouseLocation, Guid> locationRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<InventoryItem, Guid> inventoryRepository,
        IRepository<InventoryTransaction, Guid> transactionRepository)
    {
        _orderRepository = orderRepository;
        _warehouseRepository = warehouseRepository;
        _locationRepository = locationRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<PagedResultDto<InboundOrderDto>> GetListAsync(GetInboundOrderListDto input)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.OrderNumber.Contains(input.Filter!) || x.SupplierName.Contains(input.Filter!))
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId!.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status!.Value)
            .WhereIf(input.ExpectedDateFrom.HasValue, x => x.ExpectedArrivalDate >= input.ExpectedDateFrom!.Value)
            .WhereIf(input.ExpectedDateTo.HasValue, x => x.ExpectedArrivalDate <= input.ExpectedDateTo!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var pagedQuery = query
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var orders = await AsyncExecuter.ToListAsync(pagedQuery);

        var dtos = await MapToDtosAsync(orders);

        return new PagedResultDto<InboundOrderDto>(totalCount, dtos);
    }

    public async Task<InboundOrderDto> GetAsync(Guid id)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));

        if (order == null)
        {
            throw new EntityNotFoundException(typeof(InboundOrder), id);
        }

        var dtos = await MapToDtosAsync(new List<InboundOrder> { order });
        return dtos.First();
    }

    public async Task<InboundOrderDto> CreateAsync(CreateUpdateInboundOrderDto input)
    {
        var existing = await _orderRepository.FirstOrDefaultAsync(x => x.OrderNumber == input.OrderNumber.Trim().ToUpperInvariant());
        if (existing != null)
        {
            throw new UserFriendlyException($"Inbound order number '{input.OrderNumber}' already exists.");
        }

        var order = new InboundOrder(
            GuidGenerator.Create(),
            input.OrderNumber,
            input.WarehouseId,
            input.SupplierName,
            input.ExpectedArrivalDate,
            CurrentTenant.Id,
            input.SupplierInvoiceNumber,
            input.Status,
            input.Notes
        );

        if (input.Lines != null)
        {
            foreach (var lineDto in input.Lines)
            {
                order.AddLine(GuidGenerator.Create(), lineDto.ProductId, lineDto.ExpectedQuantity, lineDto.UnitPrice);
            }
        }

        await _orderRepository.InsertAsync(order, autoSave: true);

        return await GetAsync(order.Id);
    }

    public async Task<InboundOrderDto> UpdateAsync(Guid id, CreateUpdateInboundOrderDto input)
    {
        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));

        if (order == null)
        {
            throw new EntityNotFoundException(typeof(InboundOrder), id);
        }

        if (order.Status == InboundOrderStatus.Completed)
        {
            throw new UserFriendlyException("Completed inbound orders cannot be edited.");
        }

        order.SetOrderNumber(input.OrderNumber);
        order.WarehouseId = input.WarehouseId;
        order.SetSupplierName(input.SupplierName);
        order.SupplierInvoiceNumber = input.SupplierInvoiceNumber?.Trim();
        order.ExpectedArrivalDate = input.ExpectedArrivalDate;
        order.Status = input.Status;
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

        if (order.Lines.Any(x => x.ReceivedQuantity > 0))
        {
            throw new UserFriendlyException("Cannot delete an inbound order that has already received inventory.");
        }

        await _orderRepository.DeleteAsync(order, autoSave: true);
    }

    public async Task<InboundOrderDto> ReceiveGoodsAsync(ReceiveInboundGoodsDto input)
    {
        if (input.ReceivedQuantity <= 0)
        {
            throw new UserFriendlyException("Received quantity must be greater than zero.");
        }

        var query = await _orderRepository.WithDetailsAsync(x => x.Lines);
        var order = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == input.InboundOrderId));

        if (order == null)
        {
            throw new EntityNotFoundException(typeof(InboundOrder), input.InboundOrderId);
        }

        var line = order.Lines.FirstOrDefault(x => x.Id == input.InboundOrderLineId);
        if (line == null)
        {
            throw new UserFriendlyException("Inbound order line not found.");
        }

        var location = await _locationRepository.GetAsync(input.DestinationLocationId);
        if (location.WarehouseId != order.WarehouseId)
        {
            throw new UserFriendlyException("Target destination location does not belong to the order warehouse.");
        }

        // 1. Update line and order status
        order.ReceiveItem(line.Id, input.ReceivedQuantity);
        await _orderRepository.UpdateAsync(order, autoSave: true);

        // 2. Deposit into InventoryItem
        var normalizedLot = input.LotBatchNumber?.Trim();
        var normalizedSerial = input.SerialNumber?.Trim();
        var normalizedLpn = input.Lpn?.Trim();

        var invQuery = await _inventoryRepository.GetQueryableAsync();
        var matchInvQuery = invQuery.Where(x =>
            x.WarehouseId == order.WarehouseId &&
            x.LocationId == input.DestinationLocationId &&
            x.ProductId == line.ProductId &&
            x.LotBatchNumber == normalizedLot &&
            x.SerialNumber == normalizedSerial &&
            x.Lpn == normalizedLpn);

        var existingStock = await AsyncExecuter.FirstOrDefaultAsync(matchInvQuery);

        if (existingStock == null)
        {
            var newStock = new InventoryItem(
                GuidGenerator.Create(),
                order.WarehouseId,
                location.ZoneId,
                input.DestinationLocationId,
                line.ProductId,
                input.ReceivedQuantity,
                CurrentTenant.Id,
                lotBatchNumber: normalizedLot,
                serialNumber: normalizedSerial,
                lpn: normalizedLpn,
                expiryDate: input.ExpiryDate,
                manufactureDate: input.ManufactureDate,
                status: InventoryStatus.Available,
                receivedDate: DateTime.UtcNow
            );

            await _inventoryRepository.InsertAsync(newStock, autoSave: true);
        }
        else
        {
            existingStock.AdjustQuantity(input.ReceivedQuantity);
            if (input.ExpiryDate.HasValue) existingStock.ExpiryDate = input.ExpiryDate;
            if (input.ManufactureDate.HasValue) existingStock.ManufactureDate = input.ManufactureDate;
            await _inventoryRepository.UpdateAsync(existingStock, autoSave: true);
        }

        // 3. Record Audit Transaction
        var transaction = new InventoryTransaction(
            GuidGenerator.Create(),
            order.WarehouseId,
            line.ProductId,
            input.ReceivedQuantity,
            InventoryTransactionType.InboundReceipt,
            CurrentTenant.Id,
            fromLocationId: null,
            toLocationId: input.DestinationLocationId,
            lotBatchNumber: normalizedLot,
            serialNumber: normalizedSerial,
            lpn: normalizedLpn,
            referenceNumber: order.OrderNumber,
            notes: input.Notes ?? $"Received against Inbound Order {order.OrderNumber}"
        );

        await _transactionRepository.InsertAsync(transaction, autoSave: true);

        return await GetAsync(order.Id);
    }

    private async Task<List<InboundOrderDto>> MapToDtosAsync(List<InboundOrder> orders)
    {
        if (!orders.Any()) return new List<InboundOrderDto>();

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

                return new InboundOrderLineDto
                {
                    Id = line.Id,
                    InboundOrderId = line.InboundOrderId,
                    ProductId = line.ProductId,
                    ProductSku = prod?.Sku,
                    ProductName = prod?.Name,
                    ExpectedQuantity = line.ExpectedQuantity,
                    ReceivedQuantity = line.ReceivedQuantity,
                    UnitPrice = line.UnitPrice,
                    IsCompleted = line.IsCompleted
                };
            }).ToList();

            return new InboundOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                WarehouseId = order.WarehouseId,
                WarehouseCode = wh?.Code,
                WarehouseName = wh?.Name,
                SupplierName = order.SupplierName,
                SupplierInvoiceNumber = order.SupplierInvoiceNumber,
                ExpectedArrivalDate = order.ExpectedArrivalDate,
                ActualArrivalDate = order.ActualArrivalDate,
                Status = order.Status,
                Notes = order.Notes,
                TotalLines = order.TotalLines,
                TotalExpectedQuantity = order.TotalExpectedQuantity,
                TotalReceivedQuantity = order.TotalReceivedQuantity,
                Lines = lineDtos,
                CreationTime = order.CreationTime,
                CreatorId = order.CreatorId,
                LastModificationTime = order.LastModificationTime,
                LastModifierId = order.LastModifierId
            };
        }).ToList();
    }
}
