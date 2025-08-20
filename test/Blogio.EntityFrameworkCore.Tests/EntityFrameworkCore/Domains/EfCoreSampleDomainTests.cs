using Blogio.Samples;
using Xunit;

namespace Blogio.EntityFrameworkCore.Domains;

[Collection(BlogioTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<BlogioEntityFrameworkCoreTestModule>
{

}
