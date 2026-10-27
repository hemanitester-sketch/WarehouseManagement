using WarehouseManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace WarehouseManagement.Permissions;

public class WarehouseManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(WarehouseManagementPermissions.GroupName);
        var warehousePermission = myGroup.AddPermission(WarehouseManagementPermissions.Warehouses.Default, L("Permission:Warehouses"));
        warehousePermission.AddChild(WarehouseManagementPermissions.Warehouses.Create, L("Permission:Warehouses.Create"));
        warehousePermission.AddChild(WarehouseManagementPermissions.Warehouses.Edit, L("Permission:Warehouses.Edit"));
        warehousePermission.AddChild(WarehouseManagementPermissions.Warehouses.Delete, L("Permission:Warehouses.Delete"));

        var zonePermission = myGroup.AddPermission(WarehouseManagementPermissions.Zones.Default, L("Permission:Zones"));
        zonePermission.AddChild(WarehouseManagementPermissions.Zones.Create, L("Permission:Zones.Create"));
        zonePermission.AddChild(WarehouseManagementPermissions.Zones.Edit, L("Permission:Zones.Edit"));
        zonePermission.AddChild(WarehouseManagementPermissions.Zones.Delete, L("Permission:Zones.Delete"));

        var locationPermission = myGroup.AddPermission(WarehouseManagementPermissions.Locations.Default, L("Permission:Locations"));
        locationPermission.AddChild(WarehouseManagementPermissions.Locations.Create, L("Permission:Locations.Create"));
        locationPermission.AddChild(WarehouseManagementPermissions.Locations.Edit, L("Permission:Locations.Edit"));
        locationPermission.AddChild(WarehouseManagementPermissions.Locations.Delete, L("Permission:Locations.Delete"));

        var productPermission = myGroup.AddPermission(WarehouseManagementPermissions.Products.Default, L("Permission:Products"));
        productPermission.AddChild(WarehouseManagementPermissions.Products.Create, L("Permission:Products.Create"));
        productPermission.AddChild(WarehouseManagementPermissions.Products.Edit, L("Permission:Products.Edit"));
        productPermission.AddChild(WarehouseManagementPermissions.Products.Delete, L("Permission:Products.Delete"));

        var inventoryPermission = myGroup.AddPermission(WarehouseManagementPermissions.Inventory.Default, L("Permission:Inventory"));
        inventoryPermission.AddChild(WarehouseManagementPermissions.Inventory.Adjust, L("Permission:Inventory.Adjust"));
        inventoryPermission.AddChild(WarehouseManagementPermissions.Inventory.Transfer, L("Permission:Inventory.Transfer"));

        var inboundPermission = myGroup.AddPermission(WarehouseManagementPermissions.Inbound.Default, L("Permission:Inbound"));
        inboundPermission.AddChild(WarehouseManagementPermissions.Inbound.Create, L("Permission:Inbound.Create"));
        inboundPermission.AddChild(WarehouseManagementPermissions.Inbound.Edit, L("Permission:Inbound.Edit"));
        inboundPermission.AddChild(WarehouseManagementPermissions.Inbound.Delete, L("Permission:Inbound.Delete"));
        inboundPermission.AddChild(WarehouseManagementPermissions.Inbound.Receive, L("Permission:Inbound.Receive"));

        var outboundPermission = myGroup.AddPermission(WarehouseManagementPermissions.Outbound.Default, L("Permission:Outbound"));
        outboundPermission.AddChild(WarehouseManagementPermissions.Outbound.Create, L("Permission:Outbound.Create"));
        outboundPermission.AddChild(WarehouseManagementPermissions.Outbound.Edit, L("Permission:Outbound.Edit"));
        outboundPermission.AddChild(WarehouseManagementPermissions.Outbound.Delete, L("Permission:Outbound.Delete"));
        outboundPermission.AddChild(WarehouseManagementPermissions.Outbound.Allocate, L("Permission:Outbound.Allocate"));
        outboundPermission.AddChild(WarehouseManagementPermissions.Outbound.Pick, L("Permission:Outbound.Pick"));
        outboundPermission.AddChild(WarehouseManagementPermissions.Outbound.Ship, L("Permission:Outbound.Ship"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<WarehouseManagementResource>(name);
    }
}
