using Microsoft.AspNetCore.Mvc;
using R10.Web.Models;

namespace R10.Web.ViewComponents
{
    public class GridActionDeDocketHistory : ViewComponent
    {
        public IViewComponentResult Invoke(GridOptions model)
        {
            return View(model);
        }
    }

    
}
