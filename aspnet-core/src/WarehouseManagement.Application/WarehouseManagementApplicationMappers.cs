using Riok.Mapperly.Abstractions;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement;

[Mapper]
public partial class WarehouseManagementApplicationMappers
{
    public partial WarehouseDto Map(Warehouse source);
}
