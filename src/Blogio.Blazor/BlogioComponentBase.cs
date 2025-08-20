using Blogio.Localization;
using Volo.Abp.AspNetCore.Components;

namespace Blogio.Blazor;

public abstract class BlogioComponentBase : AbpComponentBase
{
    protected BlogioComponentBase()
    {
        LocalizationResource = typeof(BlogioResource);
    }
}
