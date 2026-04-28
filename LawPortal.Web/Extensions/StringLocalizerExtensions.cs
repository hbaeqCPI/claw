using Microsoft.Extensions.Localization;
using LawPortal.Web.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Extensions
{
    public static class StringLocalizerExtensions
    {
        public static string GetStringWithCulture<T>(this IStringLocalizer<T> localizer, string name, string languageCulture)
        {
            var currentCulture = CultureInfo.CurrentCulture;
            var currentUICulture = CultureInfo.CurrentUICulture;
            var cultureName = languageCulture ?? CultureInfo.CurrentCulture.Name;

            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);

            var localizedString = localizer[name];

            CultureInfo.CurrentCulture = currentCulture;
            CultureInfo.CurrentUICulture = currentUICulture;

            return localizedString;
        }
    }
}
