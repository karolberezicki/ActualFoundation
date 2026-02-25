#nullable enable
using EPiServer.Security;
using EPiServer.Shell;

namespace Foundation.Infrastructure.Cms;

[MenuProvider]
public class CmsMenuProvider : IMenuProvider
{
    private const string _mainMenuPath = MenuPaths.Global + "/extensions";

    public IEnumerable<MenuItem> GetMenuItems()
    {
        var menuItems = new List<MenuItem>
        {
            new SectionMenuItem("Extensions", _mainMenuPath)
            {
                IsAvailable = _ => PrincipalInfo.CurrentPrincipal.IsInRole("CommerceAdmins"),
                SortIndex = 6000,
            },
            new UrlMenuItem("Bulk Update", _mainMenuPath + "/bulkupdate", "/episerver/foundation/bulkupdate")
            {
                SortIndex = 100,
            },
            new FoundationAdminMenuItem("Coupons", _mainMenuPath + "/coupons", "/episerver/foundation/promotions")
            {
                SortIndex = 200,
                Paths = ["foundation/promotions", "foundation/editPromotionCoupons"],
            },
        };

        return menuItems;
    }
}

public class FoundationAdminMenuItem : UrlMenuItem
{
    public IEnumerable<string> Paths { get; set; } = [];

    public FoundationAdminMenuItem(string text, string path, string url) : base(text, path, url)
    {
    }

    public override bool IsSelected(HttpContext? requestContext)
    {
        Validate.RequiredParameter("requestContext", requestContext);
        var requestUrl = requestContext?.Request.Path.Value?.Trim('/');
        return Paths.Any(x =>  requestUrl?.Contains(x) ?? false);
    }
}