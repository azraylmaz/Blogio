using Volo.Abp.Settings;

namespace Blogio.Settings;

public class BlogioSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(BlogioSettings.MySetting1));
    }
}
