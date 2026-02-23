using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{
    public class PrintConfirmDialog : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
