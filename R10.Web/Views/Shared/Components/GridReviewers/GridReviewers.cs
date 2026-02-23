using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Web.Models;

namespace R10.Web.ViewComponents
{
    public class GridReviewers : ViewComponent
    {
        public IViewComponentResult Invoke(GridOptions model)
        {
            return View(model);
        }
    }
}
