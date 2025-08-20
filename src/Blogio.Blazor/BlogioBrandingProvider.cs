using Microsoft.Extensions.Localization;
using Blogio.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Blogio.Blazor;

[Dependency(ReplaceServices = true)]
public class BlogioBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<BlogioResource> _localizer;

    public BlogioBrandingProvider(IStringLocalizer<BlogioResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
