using WarehouseManagement.Samples;
using Xunit;

namespace WarehouseManagement.EntityFrameworkCore.Applications;

[Collection(WarehouseManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<WarehouseManagementEntityFrameworkCoreTestModule>
{

}
