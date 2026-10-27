namespace WarehouseManagement.Permissions;

public static class WarehouseManagementPermissions
{
    public const string GroupName = "WarehouseManagement";

    public static class Warehouses
    {
        public const string Default = GroupName + ".Warehouses";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Zones
    {
        public const string Default = GroupName + ".Zones";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Locations
    {
        public const string Default = GroupName + ".Locations";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Inventory
    {
        public const string Default = GroupName + ".Inventory";
        public const string Adjust = Default + ".Adjust";
        public const string Transfer = Default + ".Transfer";
    }

    public static class Inbound
    {
        public const string Default = GroupName + ".Inbound";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Receive = Default + ".Receive";
    }

    public static class Outbound
    {
        public const string Default = GroupName + ".Outbound";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Allocate = Default + ".Allocate";
        public const string Pick = Default + ".Pick";
        public const string Ship = Default + ".Ship";
    }
}
