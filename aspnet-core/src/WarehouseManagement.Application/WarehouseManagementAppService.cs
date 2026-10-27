using System;
using System.Collections.Generic;
using System.Text;
using WarehouseManagement.Localization;
using Volo.Abp.Application.Services;

namespace WarehouseManagement;

/* Inherit your application services from this class.
 */
public abstract class WarehouseManagementAppService : ApplicationService
{
    protected WarehouseManagementAppService()
    {
        LocalizationResource = typeof(WarehouseManagementResource);
    }
}
