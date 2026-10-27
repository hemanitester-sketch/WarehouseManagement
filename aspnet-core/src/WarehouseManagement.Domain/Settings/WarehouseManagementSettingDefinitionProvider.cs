using Volo.Abp.Settings;

namespace WarehouseManagement.Settings;

public class WarehouseManagementSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(WarehouseManagementSettings.MySetting1));
    }
}
