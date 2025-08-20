using System.Threading.Tasks;

namespace Blogio.Data;

public interface IBlogioDbSchemaMigrator
{
    Task MigrateAsync();
}
