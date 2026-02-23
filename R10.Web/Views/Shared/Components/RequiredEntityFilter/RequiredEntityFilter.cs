using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class RequiredEntityFilter : ViewComponent
    {
        public IViewComponentResult Invoke(string label)
        {
            return View(model: label);
        }
    }
}
