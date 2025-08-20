using Blogio.Samples;
using Xunit;

namespace Blogio.EntityFrameworkCore.Applications;

[Collection(BlogioTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<BlogioEntityFrameworkCoreTestModule>
{

}
