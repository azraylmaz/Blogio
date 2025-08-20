using Volo.Abp.Modularity;

namespace Blogio;

[DependsOn(
    typeof(BlogioApplicationModule),
    typeof(BlogioDomainTestModule)
)]
public class BlogioApplicationTestModule : AbpModule
{

}
