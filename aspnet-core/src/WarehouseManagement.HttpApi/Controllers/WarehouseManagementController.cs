using WarehouseManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace WarehouseManagement.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class WarehouseManagementController : AbpControllerBase
{
    protected WarehouseManagementController()
    {
        LocalizationResource = typeof(WarehouseManagementResource);
    }
}
