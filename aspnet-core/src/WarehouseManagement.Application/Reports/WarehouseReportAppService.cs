using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WarehouseManagement.Audits;
using WarehouseManagement.Customers;
using WarehouseManagement.Inbound;
using WarehouseManagement.Inventory;
using WarehouseManagement.Outbound;
using WarehouseManagement.Products;
using WarehouseManagement.Returns;
using WarehouseManagement.Suppliers;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Reports;

public class WarehouseReportAppService : ApplicationService, IWarehouseReportAppService
{
    private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
    private readonly IRepository<InventoryTransaction, Guid> _transactionRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<WarehouseLocation, Guid> _locationRepository;
    private readonly IRepository<WarehouseZone, Guid> _zoneRepository;
    private readonly IRepository<InboundOrder, Guid> _inboundRepository;
    private readonly IRepository<OutboundOrder, Guid> _outboundRepository;
    private readonly IRepository<CycleCountAudit, Guid> _auditRepository;
    private readonly IRepository<ReturnOrder, Guid> _returnRepository;
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;

    public WarehouseReportAppService(
        IRepository<InventoryItem, Guid> inventoryRepository,
        IRepository<InventoryTransaction, Guid> transactionRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<Category, Guid> categoryRepository,
        IRepository<Warehouse, Guid> warehouseRepository,
        IRepository<WarehouseLocation, Guid> locationRepository,
        IRepository<WarehouseZone, Guid> zoneRepository,
        IRepository<InboundOrder, Guid> inboundRepository,
        IRepository<OutboundOrder, Guid> outboundRepository,
        IRepository<CycleCountAudit, Guid> auditRepository,
        IRepository<ReturnOrder, Guid> returnRepository,
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<Customer, Guid> customerRepository)
    {
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _warehouseRepository = warehouseRepository;
        _locationRepository = locationRepository;
        _zoneRepository = zoneRepository;
        _inboundRepository = inboundRepository;
        _outboundRepository = outboundRepository;
        _auditRepository = auditRepository;
        _returnRepository = returnRepository;
        _supplierRepository = supplierRepository;
        _customerRepository = customerRepository;
    }

    public async Task<ExecutiveWarehouseKpisDto> GetExecutiveKpisAsync(GetWarehouseReportFilterDto input)
    {
        // 1. Inventory & Valuation
        var invQueryable = await _inventoryRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            invQueryable = invQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }

        var inventoryItems = await AsyncExecuter.ToListAsync(invQueryable);
        var productIds = inventoryItems.Select(x => x.ProductId).Distinct().ToList();

        var products = await _productRepository.GetListAsync(x => productIds.Contains(x.Id) || x.IsActive);
        var productMap = products.ToDictionary(x => x.Id, x => x);

        decimal totalValuation = 0;
        decimal totalUnits = 0;
        var stockPerProduct = new Dictionary<Guid, decimal>();

        foreach (var item in inventoryItems)
        {
            totalUnits += item.QuantityOnHand;
            var price = productMap.TryGetValue(item.ProductId, out var prod) ? prod.UnitPrice : 0;
            totalValuation += item.QuantityOnHand * price;

            if (stockPerProduct.ContainsKey(item.ProductId))
                stockPerProduct[item.ProductId] += item.QuantityOnHand;
            else
                stockPerProduct[item.ProductId] = item.QuantityOnHand;
        }

        var activeProducts = products.Where(x => x.IsActive).ToList();
        var totalActiveSkus = activeProducts.Count;
        var lowStockCount = activeProducts.Count(p =>
        {
            stockPerProduct.TryGetValue(p.Id, out var qty);
            return qty <= p.ReorderLevel;
        });

        // 2. Space & Locations
        var locQueryable = await _locationRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            locQueryable = locQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        var locations = await AsyncExecuter.ToListAsync(locQueryable);
        var totalLocations = locations.Count;
        var occupiedLocationIds = inventoryItems.Where(x => x.QuantityOnHand > 0).Select(x => x.LocationId).ToHashSet();
        var occupiedCount = locations.Count(x => x.Status == WarehouseLocationStatus.Occupied || occupiedLocationIds.Contains(x.Id));
        var spaceUtilRate = totalLocations > 0
            ? Math.Round((double)occupiedCount / totalLocations * 100.0, 1)
            : 0;

        // 3. Inbound Throughput & Queues
        var inboundQueryable = await _inboundRepository.WithDetailsAsync(x => x.Lines);
        if (input.WarehouseId.HasValue)
        {
            inboundQueryable = inboundQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var inbounds = await AsyncExecuter.ToListAsync(inboundQueryable);
        
        var recentInbounds = inbounds.Where(x => x.CreationTime >= thirtyDaysAgo).ToList();
        decimal inboundThroughput = recentInbounds.SelectMany(x => x.Lines).Sum(x => x.ReceivedQuantity);
        var pendingInbounds = inbounds.Count(x => x.Status != InboundOrderStatus.Completed && x.Status != InboundOrderStatus.Cancelled);

        // 4. Outbound Throughput & Queues
        var outboundQueryable = await _outboundRepository.WithDetailsAsync(x => x.Lines);
        if (input.WarehouseId.HasValue)
        {
            outboundQueryable = outboundQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        var outbounds = await AsyncExecuter.ToListAsync(outboundQueryable);
        var recentOutbounds = outbounds.Where(x => x.CreationTime >= thirtyDaysAgo && x.Status == OutboundOrderStatus.Shipped).ToList();
        decimal outboundThroughput = recentOutbounds.SelectMany(x => x.Lines).Sum(x => x.PickedQuantity > 0 ? x.PickedQuantity : x.OrderedQuantity);
        var pendingOutbounds = outbounds.Count(x => x.Status != OutboundOrderStatus.Shipped && x.Status != OutboundOrderStatus.Cancelled);

        // 5. Audit Accuracy
        var auditQueryable = await _auditRepository.WithDetailsAsync(x => x.Items);
        if (input.WarehouseId.HasValue)
        {
            auditQueryable = auditQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        var audits = await AsyncExecuter.ToListAsync(auditQueryable);
        var auditItems = audits.SelectMany(x => x.Items).ToList();
        double auditAccuracy = 100.0;
        if (auditItems.Any())
        {
            var correctCount = auditItems.Count(x => x.VarianceQuantity == 0);
            auditAccuracy = Math.Round((double)correctCount / auditItems.Count * 100.0, 1);
        }

        // 6. Returns Rate & Salvage
        var returnQueryable = await _returnRepository.WithDetailsAsync(x => x.Items);
        if (input.WarehouseId.HasValue)
        {
            returnQueryable = returnQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        var returns = await AsyncExecuter.ToListAsync(returnQueryable);
        decimal totalReturnedQty = returns.SelectMany(x => x.Items).Sum(x => x.ExpectedQuantity);
        decimal totalShippedAll = outbounds.Where(x => x.Status == OutboundOrderStatus.Shipped)
                                           .SelectMany(x => x.Lines).Sum(x => x.OrderedQuantity);
        double returnRate = totalShippedAll > 0
            ? Math.Round((double)(totalReturnedQty / totalShippedAll) * 100.0, 2)
            : 0;

        decimal totalScrap = returns.SelectMany(x => x.Items).Sum(x => x.ScrappedQuantity * x.UnitRefundPrice);

        return new ExecutiveWarehouseKpisDto
        {
            TotalInventoryValuation = Math.Round(totalValuation, 2),
            TotalStockUnits = totalUnits,
            TotalActiveSkus = totalActiveSkus,
            LowStockSkuCount = lowStockCount,
            OverallSpaceUtilizationRate = spaceUtilRate,
            TotalOccupiedBins = occupiedCount,
            TotalWarehouseLocations = totalLocations,
            InboundThroughput30Days = inboundThroughput,
            OutboundThroughput30Days = outboundThroughput,
            AuditAccuracyRate = auditAccuracy,
            ReturnRatePercentage = returnRate,
            PendingInboundOrders = pendingInbounds,
            PendingOutboundOrders = pendingOutbounds,
            TotalSalvageOrScrapValue = Math.Round(totalScrap, 2)
        };
    }

    public async Task<List<StockAgingReportItemDto>> GetStockAgingReportAsync(GetWarehouseReportFilterDto input)
    {
        var invQueryable = await _inventoryRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            invQueryable = invQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }

        var inventoryItems = await AsyncExecuter.ToListAsync(invQueryable.Where(x => x.QuantityOnHand > 0));
        if (!inventoryItems.Any())
        {
            return new List<StockAgingReportItemDto>();
        }

        var productIds = inventoryItems.Select(x => x.ProductId).Distinct().ToList();
        var warehouseIds = inventoryItems.Select(x => x.WarehouseId).Distinct().ToList();

        var products = await _productRepository.GetListAsync(x => productIds.Contains(x.Id));
        var productMap = products.ToDictionary(x => x.Id, x => x);

        var categories = await _categoryRepository.GetListAsync();
        var categoryMap = categories.ToDictionary(x => x.Id, x => x.Name);

        var warehouses = await _warehouseRepository.GetListAsync(x => warehouseIds.Contains(x.Id));
        var warehouseMap = warehouses.ToDictionary(x => x.Id, x => x.Name);

        // Calculate 90 days outbound shipped per product for FSN classification
        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
        var outboundQueryable = await _outboundRepository.WithDetailsAsync(x => x.Lines);
        var outbounds = await AsyncExecuter.ToListAsync(outboundQueryable.Where(x => x.CreationTime >= ninetyDaysAgo));
        var shippedVelocityMap = outbounds
            .SelectMany(x => x.Lines)
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.PickedQuantity > 0 ? x.PickedQuantity : x.OrderedQuantity));

        // Group inventory by Product and Warehouse
        var grouped = inventoryItems
            .GroupBy(x => new { x.ProductId, x.WarehouseId })
            .Select(g =>
            {
                var totalQty = g.Sum(x => x.QuantityOnHand);
                var oldestDate = g.Min(x => x.ReceivedDate != default ? x.ReceivedDate : x.CreationTime);
                var days = (int)(DateTime.UtcNow - oldestDate).TotalDays;
                if (days < 0) days = 0;

                string bracket = days switch
                {
                    <= 30 => "0-30 Days (Fresh)",
                    <= 60 => "31-60 Days (Moderate)",
                    <= 90 => "61-90 Days (Aging)",
                    _ => "90+ Days (Dead Stock)"
                };

                shippedVelocityMap.TryGetValue(g.Key.ProductId, out var velocity);

                string fsn = velocity switch
                {
                    >= 50 => "Fast-Moving (F)",
                    > 0 => "Slow-Moving (S)",
                    _ => "Non-Moving (N)"
                };

                productMap.TryGetValue(g.Key.ProductId, out var product);
                warehouseMap.TryGetValue(g.Key.WarehouseId, out var whName);

                string catName = "General Goods";
                if (product?.CategoryId.HasValue == true && categoryMap.TryGetValue(product.CategoryId.Value, out var cName))
                {
                    catName = cName;
                }
                else if (product != null)
                {
                    catName = product.Category.ToString();
                }

                var unitPrice = product?.UnitPrice ?? 0;

                return new StockAgingReportItemDto
                {
                    ProductId = g.Key.ProductId,
                    Sku = product?.Sku ?? "UNKNOWN",
                    ProductName = product?.Name ?? "Unknown Product",
                    CategoryName = catName,
                    WarehouseId = g.Key.WarehouseId,
                    WarehouseName = whName ?? "Main Warehouse",
                    QuantityOnHand = totalQty,
                    UnitPrice = unitPrice,
                    TotalValuation = Math.Round(totalQty * unitPrice, 2),
                    OldestReceivedDate = oldestDate,
                    AgeInDays = days,
                    AgeBracket = bracket,
                    FsnClassification = fsn,
                    OutboundShippedLast90Days = velocity
                };
            })
            .OrderByDescending(x => x.AgeInDays)
            .ToList();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var f = input.Filter.Trim().ToLower();
            grouped = grouped.Where(x =>
                x.Sku.ToLower().Contains(f) ||
                x.ProductName.ToLower().Contains(f) ||
                x.CategoryName.ToLower().Contains(f) ||
                x.WarehouseName.ToLower().Contains(f) ||
                x.AgeBracket.ToLower().Contains(f) ||
                x.FsnClassification.ToLower().Contains(f)).ToList();
        }

        return grouped;
    }

    public async Task<List<SpaceUtilizationReportItemDto>> GetSpaceUtilizationReportAsync(GetWarehouseReportFilterDto input)
    {
        var whQueryable = await _warehouseRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            whQueryable = whQueryable.Where(x => x.Id == input.WarehouseId.Value);
        }
        var warehouses = await AsyncExecuter.ToListAsync(whQueryable);
        var warehouseIds = warehouses.Select(x => x.Id).ToList();

        var zones = await _zoneRepository.GetListAsync(x => warehouseIds.Contains(x.WarehouseId));
        var locations = await _locationRepository.GetListAsync(x => warehouseIds.Contains(x.WarehouseId));
        var inventoryItems = await _inventoryRepository.GetListAsync(x => warehouseIds.Contains(x.WarehouseId) && x.QuantityOnHand > 0);

        var occupiedLocationIds = inventoryItems.Select(x => x.LocationId).ToHashSet();

        var result = new List<SpaceUtilizationReportItemDto>();

        foreach (var wh in warehouses)
        {
            var whZones = zones.Where(x => x.WarehouseId == wh.Id).ToList();
            var whLocs = locations.Where(x => x.WarehouseId == wh.Id).ToList();
            var whInv = inventoryItems.Where(x => x.WarehouseId == wh.Id).ToList();

            var totalBins = whLocs.Count;
            var occupiedBins = whLocs.Count(x => x.Status == WarehouseLocationStatus.Occupied || occupiedLocationIds.Contains(x.Id));
            var availableBins = Math.Max(0, totalBins - occupiedBins);
            var utilPct = totalBins > 0 ? Math.Round((double)occupiedBins / totalBins * 100.0, 1) : 0;
            var totalStockUnits = whInv.Sum(x => x.QuantityOnHand);

            var zoneSummaries = whZones.Select(z =>
            {
                var zLocs = whLocs.Where(l => l.ZoneId == z.Id).ToList();
                var zTotal = zLocs.Count;
                var zOccupied = zLocs.Count(l => l.Status == WarehouseLocationStatus.Occupied || occupiedLocationIds.Contains(l.Id));
                var zUtil = zTotal > 0 ? Math.Round((double)zOccupied / zTotal * 100.0, 1) : 0;

                return new ZoneUtilizationSummaryDto
                {
                    ZoneId = z.Id,
                    ZoneName = z.Name,
                    StorageType = z.StorageType.ToString(),
                    TotalBins = zTotal,
                    OccupiedBins = zOccupied,
                    UtilizationPercentage = zUtil
                };
            }).OrderByDescending(x => x.UtilizationPercentage).ToList();

            result.Add(new SpaceUtilizationReportItemDto
            {
                WarehouseId = wh.Id,
                WarehouseCode = wh.Code,
                WarehouseName = wh.Name,
                City = wh.City,
                TotalZones = whZones.Count,
                TotalBins = totalBins,
                OccupiedBins = occupiedBins,
                AvailableBins = availableBins,
                UtilizationPercentage = utilPct,
                TotalStockUnitsStored = totalStockUnits,
                ZoneBreakdown = zoneSummaries
            });
        }

        return result.OrderByDescending(x => x.UtilizationPercentage).ToList();
    }

    public async Task<List<InboundSupplierSlaReportItemDto>> GetSupplierSlaReportAsync(GetWarehouseReportFilterDto input)
    {
        var inboundQueryable = await _inboundRepository.WithDetailsAsync(x => x.Lines);
        if (input.WarehouseId.HasValue)
        {
            inboundQueryable = inboundQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.FromDate.HasValue)
        {
            inboundQueryable = inboundQueryable.Where(x => x.CreationTime >= input.FromDate.Value);
        }
        if (input.ToDate.HasValue)
        {
            inboundQueryable = inboundQueryable.Where(x => x.CreationTime <= input.ToDate.Value);
        }

        var orders = await AsyncExecuter.ToListAsync(inboundQueryable);
        var suppliers = await _supplierRepository.GetListAsync();
        var supplierMap = suppliers.ToDictionary(x => x.Name.Trim().ToLower(), x => x);

        var grouped = orders
            .GroupBy(x => x.SupplierName.Trim())
            .Select(g =>
            {
                var supName = g.Key;
                supplierMap.TryGetValue(supName.ToLower(), out var sup);

                var totalOrders = g.Count();
                var completedOrders = g.Count(x => x.Status == InboundOrderStatus.Completed);
                var pendingOrders = g.Count(x => x.Status != InboundOrderStatus.Completed && x.Status != InboundOrderStatus.Cancelled);

                decimal expectedQty = g.SelectMany(x => x.Lines).Sum(x => x.ExpectedQuantity);
                decimal receivedQty = g.SelectMany(x => x.Lines).Sum(x => x.ReceivedQuantity);

                var fulfillPct = expectedQty > 0
                    ? Math.Round((double)(receivedQty / expectedQty) * 100.0, 1)
                    : 100.0;

                var onTimeCount = g.Count(x =>
                    x.Status == InboundOrderStatus.Completed &&
                    x.ActualArrivalDate.HasValue &&
                    x.ActualArrivalDate.Value.Date <= x.ExpectedArrivalDate.Date);

                var onTimePct = completedOrders > 0
                    ? Math.Round((double)onTimeCount / completedOrders * 100.0, 1)
                    : 100.0;

                return new InboundSupplierSlaReportItemDto
                {
                    SupplierId = sup?.Id,
                    SupplierCode = sup?.Code ?? "SUP-GEN",
                    SupplierName = supName,
                    TotalPurchaseOrders = totalOrders,
                    CompletedOrders = completedOrders,
                    PendingOrders = pendingOrders,
                    TotalExpectedQuantity = expectedQty,
                    TotalReceivedQuantity = receivedQty,
                    FulfillmentRatePercentage = fulfillPct,
                    OnTimeDeliveries = onTimeCount,
                    OnTimeDeliveryPercentage = onTimePct
                };
            })
            .OrderByDescending(x => x.TotalPurchaseOrders)
            .ToList();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var f = input.Filter.Trim().ToLower();
            grouped = grouped.Where(x =>
                x.SupplierName.ToLower().Contains(f) ||
                x.SupplierCode.ToLower().Contains(f)).ToList();
        }

        return grouped;
    }

    public async Task<OutboundFulfillmentReportItemDto> GetOutboundFulfillmentReportAsync(GetWarehouseReportFilterDto input)
    {
        var outboundQueryable = await _outboundRepository.WithDetailsAsync(x => x.Lines);
        if (input.WarehouseId.HasValue)
        {
            outboundQueryable = outboundQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.FromDate.HasValue)
        {
            outboundQueryable = outboundQueryable.Where(x => x.OrderDate >= input.FromDate.Value);
        }
        if (input.ToDate.HasValue)
        {
            outboundQueryable = outboundQueryable.Where(x => x.OrderDate <= input.ToDate.Value);
        }

        var orders = await AsyncExecuter.ToListAsync(outboundQueryable);
        var customers = await _customerRepository.GetListAsync();
        var customerMap = customers.ToDictionary(x => x.Name.Trim().ToLower(), x => x);

        var totalOrders = orders.Count;
        var shippedOrders = orders.Count(x => x.Status == OutboundOrderStatus.Shipped);
        var pendingOrders = orders.Count(x => x.Status != OutboundOrderStatus.Shipped && x.Status != OutboundOrderStatus.Cancelled);
        var cancelledOrders = orders.Count(x => x.Status == OutboundOrderStatus.Cancelled);

        decimal totalUnitsShipped = orders
            .Where(x => x.Status == OutboundOrderStatus.Shipped)
            .SelectMany(x => x.Lines)
            .Sum(x => x.PickedQuantity > 0 ? x.PickedQuantity : x.OrderedQuantity);

        var fulfillRate = totalOrders > 0
            ? Math.Round((double)shippedOrders / (totalOrders - cancelledOrders > 0 ? totalOrders - cancelledOrders : 1) * 100.0, 1)
            : 100.0;

        var onTimeCount = orders.Count(x =>
            x.Status == OutboundOrderStatus.Shipped &&
            x.ShippedDate.HasValue &&
            x.RequiredDeliveryDate.HasValue &&
            x.ShippedDate.Value.Date <= x.RequiredDeliveryDate.Value.Date);

        var onTimePct = shippedOrders > 0
            ? Math.Round((double)onTimeCount / shippedOrders * 100.0, 1)
            : 95.0;

        var topCustomers = orders
            .GroupBy(x => x.CustomerName.Trim())
            .Select(g =>
            {
                var custName = g.Key;
                customerMap.TryGetValue(custName.ToLower(), out var cust);

                var units = g.SelectMany(x => x.Lines).Sum(x => x.OrderedQuantity);
                var value = g.SelectMany(x => x.Lines).Sum(x => x.OrderedQuantity * x.UnitPrice);

                return new CustomerFulfillmentSummaryDto
                {
                    CustomerId = cust?.Id,
                    CustomerCode = cust?.Code ?? "CUST-B2B",
                    CustomerName = custName,
                    TotalOrders = g.Count(),
                    TotalUnitsDelivered = units,
                    TotalOrderValue = Math.Round(value, 2)
                };
            })
            .OrderByDescending(x => x.TotalUnitsDelivered)
            .Take(10)
            .ToList();

        return new OutboundFulfillmentReportItemDto
        {
            TotalOutboundOrders = totalOrders,
            ShippedOrders = shippedOrders,
            PendingOrders = pendingOrders,
            CancelledOrders = cancelledOrders,
            TotalUnitsShipped = totalUnitsShipped,
            FulfillmentRatePercentage = fulfillRate,
            OnTimeDispatchPercentage = onTimePct,
            TopCustomers = topCustomers
        };
    }

    public async Task<QualityReverseLogisticsReportDto> GetQualityAndReturnsReportAsync(GetWarehouseReportFilterDto input)
    {
        // 1. Audits
        var auditQueryable = await _auditRepository.WithDetailsAsync(x => x.Items);
        if (input.WarehouseId.HasValue)
        {
            auditQueryable = auditQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        var audits = await AsyncExecuter.ToListAsync(auditQueryable);
        var auditItems = audits.SelectMany(x => x.Items).ToList();

        var totalAudits = audits.Count;
        var totalAuditedUnits = auditItems.Sum(x => x.CountedQuantity ?? x.SystemQuantity);
        var netVarianceUnits = auditItems.Sum(x => x.VarianceQuantity);
        var netVarianceValue = auditItems.Sum(x => x.VarianceValue);

        double auditAccuracy = 100.0;
        if (auditItems.Any())
        {
            var accurateLines = auditItems.Count(x => x.VarianceQuantity == 0);
            auditAccuracy = Math.Round((double)accurateLines / auditItems.Count * 100.0, 1);
        }

        // 2. Returns
        var returnQueryable = await _returnRepository.WithDetailsAsync(x => x.Items);
        if (input.WarehouseId.HasValue)
        {
            returnQueryable = returnQueryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        var returns = await AsyncExecuter.ToListAsync(returnQueryable);
        var returnItems = returns.SelectMany(x => x.Items).ToList();

        var totalRmas = returns.Count;
        var totalReturnedUnits = returnItems.Sum(x => x.ExpectedQuantity);
        var totalRefund = returns.Sum(x => x.TotalRefundOrCreditAmount);
        var totalRestocked = returnItems.Sum(x => x.RestockedQuantity);
        var totalScrapped = returnItems.Sum(x => x.ScrappedQuantity);

        var recoveryRate = (totalRestocked + totalScrapped) > 0
            ? Math.Round((double)(totalRestocked / (totalRestocked + totalScrapped)) * 100.0, 1)
            : 100.0;

        var reasonBreakdown = returnItems
            .GroupBy(x => x.ReturnReason)
            .Select(g =>
            {
                var count = g.Count();
                var pct = returnItems.Count > 0
                    ? Math.Round((double)count / returnItems.Count * 100.0, 1)
                    : 0;

                return new ReasonBreakdownDto
                {
                    Reason = FormatReturnReason(g.Key),
                    Count = count,
                    Percentage = pct
                };
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        return new QualityReverseLogisticsReportDto
        {
            TotalAuditsConducted = totalAudits,
            TotalItemsAudited = totalAuditedUnits,
            NetVarianceUnits = netVarianceUnits,
            NetVarianceCostValue = Math.Round(netVarianceValue, 2),
            AuditAccuracyRatePercentage = auditAccuracy,
            TotalRmaReturns = totalRmas,
            TotalReturnedUnits = totalReturnedUnits,
            TotalRefundValue = Math.Round(totalRefund, 2),
            TotalRestockedUnits = totalRestocked,
            TotalScrappedUnits = totalScrapped,
            RestockRecoveryRatePercentage = recoveryRate,
            TopReturnReasons = reasonBreakdown
        };
    }

    private static string FormatReturnReason(ReturnReason reason)
    {
        return reason switch
        {
            ReturnReason.DefectiveItem => "Defective / Malfunctioning",
            ReturnReason.WrongItemShipped => "Wrong SKU Shipped",
            ReturnReason.DamagedInTransit => "Damaged in Transit",
            ReturnReason.CustomerCancellation => "Customer Cancellation",
            ReturnReason.QualityDiscrepancy => "Quality Discrepancy",
            ReturnReason.ExpiredProduct => "Expired Product",
            ReturnReason.ExcessStock => "Excess Stock",
            _ => "Other Discrepancies"
        };
    }
}
