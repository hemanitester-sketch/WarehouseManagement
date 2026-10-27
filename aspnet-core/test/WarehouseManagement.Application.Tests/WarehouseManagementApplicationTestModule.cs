using Volo.Abp.Modularity;

namespace WarehouseManagement;

[DependsOn(
    typeof(WarehouseManagementApplicationModule),
    typeof(WarehouseManagementDomainTestModule)
)]
public class WarehouseManagementApplicationTestModule : AbpModule
{

}
