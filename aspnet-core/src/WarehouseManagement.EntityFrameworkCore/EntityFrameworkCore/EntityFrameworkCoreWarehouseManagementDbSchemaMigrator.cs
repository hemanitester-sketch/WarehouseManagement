using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManagement.Data;
using Volo.Abp.DependencyInjection;

namespace WarehouseManagement.EntityFrameworkCore;

public class EntityFrameworkCoreWarehouseManagementDbSchemaMigrator
    : IWarehouseManagementDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreWarehouseManagementDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the WarehouseManagementDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<WarehouseManagementDbContext>()
            .Database
            .MigrateAsync();
    }
}
