using Volo.Abp.Modularity;

namespace WarehouseManagement;

[DependsOn(
    typeof(WarehouseManagementDomainModule),
    typeof(WarehouseManagementTestBaseModule)
)]
public class WarehouseManagementDomainTestModule : AbpModule
{

}
