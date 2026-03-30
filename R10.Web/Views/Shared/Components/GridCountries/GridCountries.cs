using Microsoft.AspNetCore.Mvc;
using R10.Web.Models;

namespace R10.Web.ViewComponents
{
    public class GridCountries : ViewComponent
    {
        public IViewComponentResult Invoke(GridOptions model)
        {
            if (model.Permission != null)
                ViewData["PagePermission"] = model.Permission;
            return View(model);
        }
    }
}
