using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using LawPortal.Core.Entities;
using LawPortal.Core.Helpers;
using LawPortal.Core.Identity;
using LawPortal.Web.Models.PageViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static async Task<IHtmlContent> PartialPageAsync(this IHtmlHelper htmlHelper, PageType pageType, object model)
        {
            return await htmlHelper.PartialAsync(GetPage(pageType), model);
        }

        public static string GetPage(PageType pageType)
        {
            string page = "~/Views/Shared/_SearchPage.cshtml";

            switch (pageType)
            {
                case PageType.Search:
                    page = "~/Views/Shared/_SearchPage.cshtml";
                    break;

                case PageType.SearchResults:
                    page = "~/Views/Shared/_SearchResultsPage.cshtml";
                    break;

                case PageType.Detail:
                    page = "~/Views/Shared/_DetailPage.cshtml";
                    break;

                case PageType.DetailContent:
                    page = "~/Views/Shared/_DetailContentPage.cshtml";
                    break;

                case PageType.CompactSearchResults:
                    page = "~/Views/Shared/_CompactSearchResultsPage.cshtml";
                    break;

                case PageType.CompactSearchPage:
                    page = "~/Views/Shared/_CompactSearchPage.cshtml";
                    break;
            }
            return page;
        }

        public static string GetDetailsLinkTemplate(this IHtmlHelper htmlHelper, string url, string columnName)
        {
            var data = string.IsNullOrEmpty(columnName) ? "" : $"#: {columnName}#";
            return $"<a href='{url}' class='details-link'>{data}</a>";
        }

        public static string GetString(this IHtmlContent content)
        {
            using (var writer = new System.IO.StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        public static IEnumerable<SelectListItem> GetUserTypeSelectList(this IHtmlHelper htmlHelper, ClaimsPrincipal user)
        {
            var userTypes = htmlHelper.GetEnumSelectList<CPiUserType>();

            //Hide ContactPerson if AMS and DMS are disabled
            //if (!user.IsSystemEnabled(SystemType.DMS) && !user.IsSystemEnabled(SystemType.AMS))
            //Use helper to include other systems that use ContactPerson user type
            if (!user.IsSystemWithContactPersonEnabled())
                userTypes = userTypes.Where(t => t.Value != ((int)CPiUserType.ContactPerson).ToString());

            //Hide Attorney if AMS is disabled
            //if (!user.IsSystemEnabled(SystemType.AMS))
            //Use helper to include other systems that use Attorney user type
            if (!user.IsSystemWithAttorneyEnabled())
                userTypes = userTypes.Where(t => t.Value != ((int)CPiUserType.Attorney).ToString());

            //Hide Inventor if DMS is disabled
            if (!user.IsSystemEnabled(SystemType.DMS))
                userTypes = userTypes.Where(t => t.Value != ((int)CPiUserType.Inventor).ToString());

            if (!user.IsSuper())
                userTypes = userTypes.Where(t => t.Value != ((int)CPiUserType.SuperAdministrator).ToString());

            return userTypes.OrderBy(o => o.Text);
        }
    }
}
