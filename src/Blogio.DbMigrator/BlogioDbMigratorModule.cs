using Blogio.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Blogio.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(BlogioEntityFrameworkCoreModule),
    typeof(BlogioApplicationContractsModule)
    )]
public class BlogioDbMigratorModule : AbpModule
{
}
