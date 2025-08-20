using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Blogio.Data;

/* This is used if database provider does't define
 * IBlogioDbSchemaMigrator implementation.
 */
public class NullBlogioDbSchemaMigrator : IBlogioDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
