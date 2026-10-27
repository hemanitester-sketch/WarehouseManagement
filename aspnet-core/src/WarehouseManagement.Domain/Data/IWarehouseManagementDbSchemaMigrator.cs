using System.Threading.Tasks;

namespace WarehouseManagement.Data;

public interface IWarehouseManagementDbSchemaMigrator
{
    Task MigrateAsync();
}
