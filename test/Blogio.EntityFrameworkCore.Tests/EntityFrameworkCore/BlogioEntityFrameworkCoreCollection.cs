using Xunit;

namespace Blogio.EntityFrameworkCore;

[CollectionDefinition(BlogioTestConsts.CollectionDefinitionName)]
public class BlogioEntityFrameworkCoreCollection : ICollectionFixture<BlogioEntityFrameworkCoreFixture>
{

}
