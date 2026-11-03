using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using WarehouseManagement.EntityFrameworkCore;

namespace WarehouseManagement.Barcodes;

public class PostgreSqlBarcodeSequenceProvider : IBarcodeSequenceProvider, ITransientDependency
{
    private readonly IDbContextProvider<WarehouseManagementDbContext> _dbContextProvider;
    private static bool _sequenceEnsured;
    private static readonly object _lock = new();

    public PostgreSqlBarcodeSequenceProvider(IDbContextProvider<WarehouseManagementDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<long> GetNextSequenceNumberAsync(string sequenceName = "app_barcode_internal_seq")
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();

        if (!_sequenceEnsured)
        {
            lock (_lock)
            {
                if (!_sequenceEnsured)
                {
                    EnsureSequenceCreated(dbContext, sequenceName);
                    _sequenceEnsured = true;
                }
            }
        }

        var connection = dbContext.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT nextval('{sequenceName}');";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }
        finally
        {
            if (!wasOpen)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static void EnsureSequenceCreated(WarehouseManagementDbContext dbContext, string sequenceName)
    {
        try
        {
            dbContext.Database.ExecuteSqlRaw($"CREATE SEQUENCE IF NOT EXISTS {sequenceName} START WITH 100001 INCREMENT BY 1;");
        }
        catch
        {
            // Ignore if concurrent creation race condition
        }
    }
}

