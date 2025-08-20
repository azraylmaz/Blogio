using Volo.Abp.Modularity;

namespace Blogio;

public abstract class BlogioApplicationTestBase<TStartupModule> : BlogioTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
