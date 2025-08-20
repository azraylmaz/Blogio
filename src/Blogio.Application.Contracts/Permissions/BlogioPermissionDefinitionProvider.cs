using Blogio.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Blogio.Permissions;

public class BlogioPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(BlogioPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(BlogioPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BlogioResource>(name);
    }
}
