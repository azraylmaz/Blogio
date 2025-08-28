using Blogio.Localization;
using Blogio.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp.Identity.Blazor;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.TenantManagement.Blazor.Navigation;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Users;

namespace Blogio.Blazor.Menus;

public class BlogioMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<BlogioResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                BlogioMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "fas fa-home",
                order: 0
            )
        );

        context.Menu.Items.Insert(
            1,
            new ApplicationMenuItem(
                name: "Blogio.BlogPosts",
                displayName: l["Blog Posts"],
                url: "/blog",
                icon: "fas fa-blog"
            )
        );

        context.Menu.Items.Insert(
            2,
            new ApplicationMenuItem(
                name: "Blogio.Authors",
                displayName: l["Authors"],
                url: "/authors",
                icon: "fas fa-user-edit"
            )
        );

        var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();
        if (currentUser.IsInRole("admin"))
        {
            context.Menu.Items.Insert(
                3,
                new ApplicationMenuItem(
                    name: "Blogio.PendingDrafts",
                    displayName: l["Pending Drafts"],
                    url: "/admin/pending-drafts",
                    icon: "fas fa-hourglass-half"
                )
            );
        }

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenus.GroupName, 3);

        return Task.CompletedTask;
    }
}
