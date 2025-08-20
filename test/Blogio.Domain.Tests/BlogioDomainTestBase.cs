using Volo.Abp.Modularity;

namespace Blogio;

/* Inherit from this class for your domain layer tests. */
public abstract class BlogioDomainTestBase<TStartupModule> : BlogioTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
