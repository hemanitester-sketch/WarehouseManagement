using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace WarehouseManagement.Data;

/* This is used if database provider does't define
 * IWarehouseManagementDbSchemaMigrator implementation.
 */
public class NullWarehouseManagementDbSchemaMigrator : IWarehouseManagementDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
