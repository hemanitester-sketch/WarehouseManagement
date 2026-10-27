using Volo.Abp.Modularity;

namespace WarehouseManagement;

/* Inherit from this class for your domain layer tests. */
public abstract class WarehouseManagementDomainTestBase<TStartupModule> : WarehouseManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
