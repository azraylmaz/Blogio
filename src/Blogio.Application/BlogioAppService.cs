using System;
using System.Collections.Generic;
using System.Text;
using Blogio.Localization;
using Volo.Abp.Application.Services;

namespace Blogio;

/* Inherit your application services from this class.
 */
public abstract class BlogioAppService : ApplicationService
{
    protected BlogioAppService()
    {
        LocalizationResource = typeof(BlogioResource);
    }
}
