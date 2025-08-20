using Volo.Abp.Modularity;

namespace Blogio;

[DependsOn(
    typeof(BlogioDomainModule),
    typeof(BlogioTestBaseModule)
)]
public class BlogioDomainTestModule : AbpModule
{

}
