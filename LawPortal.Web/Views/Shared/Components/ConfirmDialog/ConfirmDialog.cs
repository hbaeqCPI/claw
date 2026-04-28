using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.ViewComponents
{
    public class ConfirmDialog : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
