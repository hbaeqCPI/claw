using Microsoft.AspNetCore.Mvc;
using LawPortal.Web.Models;

namespace LawPortal.Web.ViewComponents
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
