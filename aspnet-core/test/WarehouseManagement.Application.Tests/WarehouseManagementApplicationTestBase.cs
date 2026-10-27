using Volo.Abp.Modularity;

namespace WarehouseManagement;

public abstract class WarehouseManagementApplicationTestBase<TStartupModule> : WarehouseManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
