using WarehouseManagement.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace WarehouseManagement.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(WarehouseManagementEntityFrameworkCoreModule),
    typeof(WarehouseManagementApplicationContractsModule)
    )]
public class WarehouseManagementDbMigratorModule : AbpModule
{
}
