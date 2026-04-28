using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using LawPortal.Web.Areas;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LawPortal.Web.ViewComponents
{
    public class KendoCultureScripts : ViewComponent
    {
        public KendoCultureScripts() {}

        public IViewComponentResult Invoke()
        {
            //var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>(); //based on supported cultures
            //var culture = rqf.RequestCulture.Culture;
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return View(culture);
        }
    }
}


