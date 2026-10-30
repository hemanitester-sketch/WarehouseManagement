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

namespace WarehouseManagement.Audits;

public class CycleCountAuditAppService : ApplicationService, ICycleCountAuditAppService
{
    private readonly IRepository<CycleCountAudit, Guid> _auditRepository;
    private readonly IRepository<CycleCountAuditItem, Guid> _auditItemRepository;
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<WarehouseZone, Guid> _zoneRepository;
    private readonly IRepository<WarehouseLocation, Guid> _locationRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
    private readonly IRepository<InventoryTransaction, Guid> _transactionRepository;

    public CycleCountAuditAppService(
        IRepository<CycleCountAudit, Guid> auditRepository,
        IRepository<CycleCountAuditItem, Guid> auditItemRepository,
        IRepository<Warehouse, Guid> warehouseRepository,
        IRepository<WarehouseZone, Guid> zoneRepository,
        IRepository<WarehouseLocation, Guid> locationRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<InventoryItem, Guid> inventoryRepository,
        IRepository<InventoryTransaction, Guid> transactionRepository)
    {
        _auditRepository = auditRepository;
        _auditItemRepository = auditItemRepository;
        _warehouseRepository = warehouseRepository;
        _zoneRepository = zoneRepository;
        _locationRepository = locationRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<PagedResultDto<CycleCountAuditDto>> GetListAsync(GetCycleCountListDto input)
    {
        var queryable = await _auditRepository.WithDetailsAsync(x => x.Items);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim().ToLower();
            queryable = queryable.Where(x =>
                x.AuditNumber.ToLower().Contains(filter) ||
                (x.AssignedAuditorName != null && x.AssignedAuditorName.ToLower().Contains(filter)) ||
                (x.ApprovedByFacultyName != null && x.ApprovedByFacultyName.ToLower().Contains(filter)) ||
                (x.Notes != null && x.Notes.ToLower().Contains(filter)));
        }

        if (input.WarehouseId.HasValue)
        {
            queryable = queryable.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }

        if (input.ZoneId.HasValue)
        {
            queryable = queryable.Where(x => x.ZoneId == input.ZoneId.Value);
        }

        if (input.CountType.HasValue)
        {
            queryable = queryable.Where(x => x.CountType == input.CountType.Value);
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        if (input.FromDate.HasValue)
        {
            queryable = queryable.Where(x => x.ScheduledDate >= input.FromDate.Value);
        }

        if (input.ToDate.HasValue)
        {
            queryable = queryable.Where(x => x.ScheduledDate <= input.ToDate.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? "CreationTime desc"
            : input.Sorting;

        var entities = await AsyncExecuter.ToListAsync(
            queryable.OrderBy(sorting).PageBy(input.SkipCount, input.MaxResultCount)
        );

        var dtos = await MapToDtosAsync(entities);
        return new PagedResultDto<CycleCountAuditDto>(totalCount, dtos);
    }

    public async Task<CycleCountAuditDto> GetAsync(Guid id)
    {
        var queryable = await _auditRepository.WithDetailsAsync(x => x.Items);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (entity == null)
        {
            throw new UserFriendlyException($"Cycle count audit with Id '{id}' not found.");
        }

        var dtos = await MapToDtosAsync(new List<CycleCountAudit> { entity });
        return dtos.First();
    }

    public async Task<CycleCountAuditDto> CreateAsync(CreateCycleCountAuditDto input)
    {
        var warehouse = await _warehouseRepository.FindAsync(input.WarehouseId);
        if (warehouse == null)
        {
            throw new UserFriendlyException("Selected warehouse does not exist.");
        }

        // Generate Audit Number: AUD-yyyyMMdd-XXX
        var today = DateTime.UtcNow.Date;
        var todayPrefix = $"AUD-{today:yyyyMMdd}-";
        var countToday = await _auditRepository.CountAsync(x => x.AuditNumber.StartsWith(todayPrefix));
        var auditNumber = $"{todayPrefix}{(countToday + 1):D3}";

        var audit = new CycleCountAudit(
            GuidGenerator.Create(),
            auditNumber,
            input.WarehouseId,
            input.CountType,
            input.ScheduledDate,
            input.IsBlindCount,
            input.ZoneId,
            input.AssignedToFacultyId,
            input.AssignedAuditorName,
            input.Notes,
            CurrentTenant.Id);

        // If items are not passed, generate them from current inventory snapshot
        var itemsToCreate = input.Items;
        if (itemsToCreate == null || !itemsToCreate.Any())
        {
            itemsToCreate = await GenerateSnapshotItemsAsync(
                input.WarehouseId,
                input.CountType,
                input.ZoneId,
                input.SpecificProductId);
        }

        if (itemsToCreate == null || !itemsToCreate.Any())
        {
            throw new UserFriendlyException("No inventory items found matching the selected warehouse/zone/criteria to audit.");
        }

        foreach (var itm in itemsToCreate)
        {
            var auditItem = new CycleCountAuditItem(
                GuidGenerator.Create(),
                audit.Id,
                itm.ProductId,
                itm.LocationId,
                itm.SystemQuantity,
                itm.UnitCost,
                itm.LotBatchNumber,
                itm.SerialNumber);

            audit.Items.Add(auditItem);
        }

        await _auditRepository.InsertAsync(audit, autoSave: true);
        return await GetAsync(audit.Id);
    }

    public async Task<List<CreateCycleCountAuditItemDto>> GenerateSnapshotItemsAsync(
        Guid warehouseId,
        CycleCountType countType,
        Guid? zoneId = null,
        Guid? specificProductId = null)
    {
        var queryable = await _inventoryRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.WarehouseId == warehouseId);

        if (zoneId.HasValue)
        {
            queryable = queryable.Where(x => x.ZoneId == zoneId.Value);
        }

        if (specificProductId.HasValue)
        {
            queryable = queryable.Where(x => x.ProductId == specificProductId.Value);
        }

        var inventoryItems = await AsyncExecuter.ToListAsync(queryable);
        if (!inventoryItems.Any())
        {
            return new List<CreateCycleCountAuditItemDto>();
        }

        var productIds = inventoryItems.Select(x => x.ProductId).Distinct().ToList();
        var products = await _productRepository.GetListAsync(x => productIds.Contains(x.Id));
        var productCostMap = products.ToDictionary(x => x.Id, x => x.UnitPrice);

        // Filter based on count type if applicable
        if (countType == CycleCountType.RandomSampling)
        {
            var random = new Random();
            inventoryItems = inventoryItems.OrderBy(x => random.Next()).Take(15).ToList();
        }

        return inventoryItems.Select(x => new CreateCycleCountAuditItemDto
        {
            ProductId = x.ProductId,
            LocationId = x.LocationId,
            LotBatchNumber = x.LotBatchNumber,
            SerialNumber = x.SerialNumber,
            SystemQuantity = x.QuantityOnHand,
            UnitCost = productCostMap.TryGetValue(x.ProductId, out var cost) ? cost : 0
        }).ToList();
    }

    public async Task<CycleCountAuditDto> StartCountingAsync(Guid id)
    {
        var audit = await _auditRepository.GetAsync(id);
        if (audit.Status != CycleCountStatus.Draft && audit.Status != CycleCountStatus.ReCountRequested)
        {
            throw new UserFriendlyException($"Cannot start count for an audit with status '{audit.Status}'.");
        }

        audit.Status = CycleCountStatus.InProgress;
        await _auditRepository.UpdateAsync(audit, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<CycleCountAuditDto> RecordCountsAsync(Guid id, List<RecordCountItemDto> items)
    {
        var queryable = await _auditRepository.WithDetailsAsync(x => x.Items);
        var audit = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (audit == null)
        {
            throw new UserFriendlyException($"Audit with Id '{id}' not found.");
        }

        if (audit.Status == CycleCountStatus.Reconciled || audit.Status == CycleCountStatus.Cancelled)
        {
            throw new UserFriendlyException("Cannot modify counts on a finalized or cancelled audit.");
        }

        var itemMap = audit.Items.ToDictionary(x => x.Id, x => x);

        foreach (var inputItem in items)
        {
            if (itemMap.TryGetValue(inputItem.ItemId, out var entityItem))
            {
                entityItem.CountedQuantity = inputItem.CountedQuantity;
                entityItem.DiscrepancyReason = inputItem.DiscrepancyReason;
                entityItem.ReasonNotes = inputItem.ReasonNotes;
                entityItem.CountedAt = DateTime.UtcNow;
            }
        }

        RecalculateTotals(audit);
        await _auditRepository.UpdateAsync(audit, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<CycleCountAuditDto> SubmitForReviewAsync(Guid id, SubmitCountDto? input = null)
    {
        var queryable = await _auditRepository.WithDetailsAsync(x => x.Items);
        var audit = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (audit == null)
        {
            throw new UserFriendlyException($"Audit with Id '{id}' not found.");
        }

        var uncountedCount = audit.Items.Count(x => !x.CountedQuantity.HasValue);
        if (uncountedCount > 0)
        {
            throw new UserFriendlyException($"Cannot submit count for review. {uncountedCount} item(s) have not been physically counted yet.");
        }

        RecalculateTotals(audit);
        audit.Status = CycleCountStatus.UnderReview;
        if (!string.IsNullOrWhiteSpace(input?.Notes))
        {
            audit.Notes = string.IsNullOrWhiteSpace(audit.Notes)
                ? input.Notes
                : $"{audit.Notes} | {input.Notes}";
        }

        await _auditRepository.UpdateAsync(audit, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<CycleCountAuditDto> RequestReCountAsync(Guid id, string? reason = null)
    {
        var queryable = await _auditRepository.WithDetailsAsync(x => x.Items);
        var audit = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (audit == null)
        {
            throw new UserFriendlyException($"Audit with Id '{id}' not found.");
        }

        audit.Status = CycleCountStatus.ReCountRequested;

        // Flag discrepancy items for recount
        foreach (var item in audit.Items.Where(x => x.VarianceQuantity != 0))
        {
            item.IsReCounted = true;
        }

        if (!string.IsNullOrWhiteSpace(reason))
        {
            audit.Notes = string.IsNullOrWhiteSpace(audit.Notes)
                ? $"[Re-Count Requested: {reason}]"
                : $"{audit.Notes} | [Re-Count Requested: {reason}]";
        }

        await _auditRepository.UpdateAsync(audit, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<CycleCountAuditDto> ApproveAndReconcileAsync(Guid id, ApproveReconcileAuditDto? input = null)
    {
        var queryable = await _auditRepository.WithDetailsAsync(x => x.Items);
        var audit = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (audit == null)
        {
            throw new UserFriendlyException($"Audit with Id '{id}' not found.");
        }

        if (audit.Status != CycleCountStatus.UnderReview && audit.Status != CycleCountStatus.InProgress)
        {
            throw new UserFriendlyException($"Cannot reconcile audit in status '{audit.Status}'. Must be Under Review or In Progress.");
        }

        // Post Inventory Adjustments for every item with Variance != 0
        var now = DateTime.UtcNow;
        foreach (var item in audit.Items)
        {
            var variance = item.VarianceQuantity;
            if (variance == 0)
            {
                continue;
            }

            // Find matching inventory item
            var invQuery = await _inventoryRepository.GetQueryableAsync();
            var invItem = await AsyncExecuter.FirstOrDefaultAsync(
                invQuery.Where(x =>
                    x.WarehouseId == audit.WarehouseId &&
                    x.LocationId == item.LocationId &&
                    x.ProductId == item.ProductId &&
                    x.LotBatchNumber == item.LotBatchNumber));

            if (variance > 0)
            {
                // Adjustment Increase
                if (invItem != null)
                {
                    invItem.QuantityOnHand += variance;
                    await _inventoryRepository.UpdateAsync(invItem);
                }
                else
                {
                    // Create new inventory item in location
                    var loc = await _locationRepository.FindAsync(item.LocationId);
                    var newInv = new InventoryItem(
                        GuidGenerator.Create(),
                        audit.WarehouseId,
                        loc?.ZoneId ?? Guid.Empty,
                        item.LocationId,
                        item.ProductId,
                        variance,
                        CurrentTenant.Id,
                        item.LotBatchNumber,
                        item.SerialNumber,
                        status: InventoryStatus.Available,
                        receivedDate: now);

                    await _inventoryRepository.InsertAsync(newInv);
                }

                // Record transaction
                var tx = new InventoryTransaction(
                    GuidGenerator.Create(),
                    audit.WarehouseId,
                    item.ProductId,
                    variance,
                    InventoryTransactionType.AdjustmentIncrease,
                    CurrentTenant.Id,
                    toLocationId: item.LocationId,
                    lotBatchNumber: item.LotBatchNumber,
                    serialNumber: item.SerialNumber,
                    referenceNumber: audit.AuditNumber,
                    notes: $"Cycle Count Reconciliation: Discrepancy Reason = {item.DiscrepancyReason}. {item.ReasonNotes}",
                    transactionTime: now);

                await _transactionRepository.InsertAsync(tx);
            }
            else // variance < 0
            {
                var absQty = Math.Abs(variance);
                var txType = item.DiscrepancyReason == DiscrepancyReason.DamagedOrExpired
                    ? InventoryTransactionType.DamageWriteOff
                    : InventoryTransactionType.AdjustmentDecrease;

                if (invItem != null)
                {
                    invItem.QuantityOnHand = Math.Max(0, invItem.QuantityOnHand - absQty);
                    await _inventoryRepository.UpdateAsync(invItem);
                }

                // Record transaction
                var tx = new InventoryTransaction(
                    GuidGenerator.Create(),
                    audit.WarehouseId,
                    item.ProductId,
                    absQty,
                    txType,
                    CurrentTenant.Id,
                    fromLocationId: item.LocationId,
                    lotBatchNumber: item.LotBatchNumber,
                    serialNumber: item.SerialNumber,
                    referenceNumber: audit.AuditNumber,
                    notes: $"Cycle Count Write-Off: Discrepancy Reason = {item.DiscrepancyReason}. {item.ReasonNotes}",
                    transactionTime: now);

                await _transactionRepository.InsertAsync(tx);
            }
        }

        RecalculateTotals(audit);
        audit.Status = CycleCountStatus.Reconciled;
        audit.CompletedDate = now;
        audit.ApprovedDate = now;
        audit.ApprovedByFacultyName = input?.ApproverName ?? CurrentUser.UserName ?? "Warehouse Manager";
        if (!string.IsNullOrWhiteSpace(input?.ApprovalNotes))
        {
            audit.Notes = string.IsNullOrWhiteSpace(audit.Notes)
                ? $"[Approved: {input.ApprovalNotes}]"
                : $"{audit.Notes} | [Approved: {input.ApprovalNotes}]";
        }

        await _auditRepository.UpdateAsync(audit, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<CycleCountAuditDto> CancelAuditAsync(Guid id, string? reason = null)
    {
        var audit = await _auditRepository.GetAsync(id);
        if (audit.Status == CycleCountStatus.Reconciled)
        {
            throw new UserFriendlyException("Cannot cancel an already reconciled audit.");
        }

        audit.Status = CycleCountStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            audit.Notes = string.IsNullOrWhiteSpace(audit.Notes)
                ? $"[Cancelled: {reason}]"
                : $"{audit.Notes} | [Cancelled: {reason}]";
        }

        await _auditRepository.UpdateAsync(audit, autoSave: true);
        return await GetAsync(id);
    }

    public async Task<CycleCountDashboardMetricsDto> GetDashboardMetricsAsync(Guid? warehouseId = null)
    {
        var queryable = await _auditRepository.WithDetailsAsync(x => x.Items);
        if (warehouseId.HasValue)
        {
            queryable = queryable.Where(x => x.WarehouseId == warehouseId.Value);
        }

        var audits = await AsyncExecuter.ToListAsync(queryable);

        var totalAudits = audits.Count;
        var inProgress = audits.Count(x => x.Status == CycleCountStatus.InProgress || x.Status == CycleCountStatus.ReCountRequested);
        var underReview = audits.Count(x => x.Status == CycleCountStatus.UnderReview);
        var reconciled = audits.Count(x => x.Status == CycleCountStatus.Reconciled);

        var allCountedItems = audits.SelectMany(x => x.Items).Where(x => x.CountedQuantity.HasValue).ToList();
        var totalCounted = allCountedItems.Count;
        var totalAccurate = allCountedItems.Count(x => x.VarianceQuantity == 0);

        var accuracyRate = totalCounted > 0
            ? Math.Round(((decimal)totalAccurate / totalCounted) * 100m, 1)
            : 100m;

        var netVarianceUnits = audits.Sum(x => x.TotalVarianceQuantity);
        var netVarianceValue = audits.Sum(x => x.TotalVarianceValue);

        return new CycleCountDashboardMetricsDto
        {
            TotalAudits = totalAudits,
            InProgressCounts = inProgress,
            AuditsUnderReview = underReview,
            ReconciledAudits = reconciled,
            OverallAccuracyRate = accuracyRate,
            TotalNetVarianceUnits = netVarianceUnits,
            TotalNetVarianceValue = netVarianceValue
        };
    }

    private static void RecalculateTotals(CycleCountAudit audit)
    {
        audit.TotalVarianceQuantity = audit.Items.Sum(x => x.VarianceQuantity);
        audit.TotalVarianceValue = audit.Items.Sum(x => x.VarianceValue);
    }

    private async Task<List<CycleCountAuditDto>> MapToDtosAsync(List<CycleCountAudit> audits)
    {
        if (!audits.Any())
        {
            return new List<CycleCountAuditDto>();
        }

        var warehouseIds = audits.Select(x => x.WarehouseId).Distinct().ToList();
        var warehouses = await _warehouseRepository.GetListAsync(x => warehouseIds.Contains(x.Id));
        var warehouseMap = warehouses.ToDictionary(x => x.Id, x => x);

        var zoneIds = audits.Where(x => x.ZoneId.HasValue).Select(x => x.ZoneId!.Value).Distinct().ToList();
        var zones = await _zoneRepository.GetListAsync(x => zoneIds.Contains(x.Id));
        var zoneMap = zones.ToDictionary(x => x.Id, x => x);

        var allItems = audits.SelectMany(x => x.Items).ToList();
        var productIds = allItems.Select(x => x.ProductId).Distinct().ToList();
        var products = await _productRepository.GetListAsync(x => productIds.Contains(x.Id));
        var productMap = products.ToDictionary(x => x.Id, x => x);

        var locationIds = allItems.Select(x => x.LocationId).Distinct().ToList();
        var locations = await _locationRepository.GetListAsync(x => locationIds.Contains(x.Id));
        var locationMap = locations.ToDictionary(x => x.Id, x => x);

        var result = new List<CycleCountAuditDto>();

        foreach (var audit in audits)
        {
            warehouseMap.TryGetValue(audit.WarehouseId, out var wh);
            var zoneName = audit.ZoneId.HasValue && zoneMap.TryGetValue(audit.ZoneId.Value, out var zn)
                ? zn.Name
                : null;

            var itemsList = new List<CycleCountAuditItemDto>();
            foreach (var item in audit.Items)
            {
                productMap.TryGetValue(item.ProductId, out var prod);
                locationMap.TryGetValue(item.LocationId, out var loc);

                itemsList.Add(new CycleCountAuditItemDto
                {
                    Id = item.Id,
                    CycleCountAuditId = item.CycleCountAuditId,
                    ProductId = item.ProductId,
                    ProductName = prod?.Name ?? "Unknown Product",
                    ProductSku = prod?.Sku ?? "N/A",
                    ProductBarcode = prod?.Barcode,
                    LocationId = item.LocationId,
                    LocationCode = loc?.Barcode ?? (loc != null ? $"{loc.Aisle}-{loc.Rack}-{loc.Shelf}-{loc.Bin}" : "N/A"),
                    ZoneName = loc != null && zoneMap.TryGetValue(loc.ZoneId, out var locZone) ? locZone.Name : null,
                    LotBatchNumber = item.LotBatchNumber,
                    SerialNumber = item.SerialNumber,
                    SystemQuantity = item.SystemQuantity,
                    CountedQuantity = item.CountedQuantity,
                    UnitCost = item.UnitCost > 0 ? item.UnitCost : (prod?.UnitPrice ?? 0),
                    DiscrepancyReason = item.DiscrepancyReason,
                    ReasonNotes = item.ReasonNotes,
                    IsReCounted = item.IsReCounted,
                    CountedAt = item.CountedAt
                });
            }

            var totalItems = itemsList.Count;
            var countedItems = itemsList.Count(x => x.CountedQuantity.HasValue);
            var discrepancyItems = itemsList.Count(x => x.CountedQuantity.HasValue && x.VarianceQuantity != 0);
            var accurateItems = itemsList.Count(x => x.CountedQuantity.HasValue && x.VarianceQuantity == 0);
            var accuracyPct = countedItems > 0 ? Math.Round(((decimal)accurateItems / countedItems) * 100m, 1) : 100m;

            result.Add(new CycleCountAuditDto
            {
                Id = audit.Id,
                CreationTime = audit.CreationTime,
                CreatorId = audit.CreatorId,
                LastModificationTime = audit.LastModificationTime,
                LastModifierId = audit.LastModifierId,
                AuditNumber = audit.AuditNumber,
                WarehouseId = audit.WarehouseId,
                WarehouseName = wh?.Name ?? "N/A",
                WarehouseCode = wh?.Code ?? "N/A",
                ZoneId = audit.ZoneId,
                ZoneName = zoneName,
                CountType = audit.CountType,
                Status = audit.Status,
                IsBlindCount = audit.IsBlindCount,
                ScheduledDate = audit.ScheduledDate,
                CompletedDate = audit.CompletedDate,
                AssignedToFacultyId = audit.AssignedToFacultyId,
                AssignedAuditorName = audit.AssignedAuditorName,
                ApprovedByFacultyName = audit.ApprovedByFacultyName,
                ApprovedDate = audit.ApprovedDate,
                TotalVarianceQuantity = audit.TotalVarianceQuantity,
                TotalVarianceValue = audit.TotalVarianceValue,
                TotalItemsCount = totalItems,
                CountedItemsCount = countedItems,
                DiscrepancyItemsCount = discrepancyItems,
                AccuracyPercentage = accuracyPct,
                Notes = audit.Notes,
                Items = itemsList
            });
        }

        return result;
    }
}

