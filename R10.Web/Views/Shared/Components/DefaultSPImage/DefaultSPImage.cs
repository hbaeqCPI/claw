using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class DefaultSPImage : ViewComponent
    {
        public IViewComponentResult Invoke(DefaultImageViewModel model)
        {
            return View(model);
        }

    }
}
