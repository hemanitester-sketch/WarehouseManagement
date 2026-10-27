using Microsoft.Extensions.Localization;
using WarehouseManagement.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace WarehouseManagement;

[Dependency(ReplaceServices = true)]
public class WarehouseManagementBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<WarehouseManagementResource> _localizer;

    public WarehouseManagementBrandingProvider(IStringLocalizer<WarehouseManagementResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
