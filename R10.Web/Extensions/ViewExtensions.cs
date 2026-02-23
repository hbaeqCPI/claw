using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace R10.Web.Extensions
{
    public static class ViewExtensions
    {
        public static string CheckActivePage(this ViewDataDictionary viewData, string page)
        {
            var activePage = viewData["ActivePage"] as string;
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : "";
        }

        public static void AddActivePage(this ViewDataDictionary viewData, string activePage) => viewData["ActivePage"] = activePage;
    }
}
