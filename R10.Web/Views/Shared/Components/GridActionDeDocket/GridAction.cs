using Microsoft.AspNetCore.Mvc;

namespace R10.Web.ViewComponents
{
    public class GridActionDeDocket : ViewComponent
    {
        public IViewComponentResult Invoke(ActionGridOptions model)
        {
            return View(model);
        }
    }

    
}
