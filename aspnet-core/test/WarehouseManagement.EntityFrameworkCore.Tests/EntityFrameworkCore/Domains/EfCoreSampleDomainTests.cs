using WarehouseManagement.Samples;
using Xunit;

namespace WarehouseManagement.EntityFrameworkCore.Domains;

[Collection(WarehouseManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<WarehouseManagementEntityFrameworkCoreTestModule>
{

}
