using Blogio.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Blogio.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class BlogioController : AbpControllerBase
{
    protected BlogioController()
    {
        LocalizationResource = typeof(BlogioResource);
    }
}
