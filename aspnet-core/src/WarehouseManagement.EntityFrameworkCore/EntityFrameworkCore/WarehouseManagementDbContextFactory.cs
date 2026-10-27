using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WarehouseManagement.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class WarehouseManagementDbContextFactory : IDesignTimeDbContextFactory<WarehouseManagementDbContext>
{
    public WarehouseManagementDbContext CreateDbContext(string[] args)
    {
        WarehouseManagementEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<WarehouseManagementDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));

        return new WarehouseManagementDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../WarehouseManagement.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
