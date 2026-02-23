using Microsoft.AspNetCore.Mvc;
using R10.Web.Models;

namespace R10.Web.ViewComponents
{
    public class GridRelatedMatter : ViewComponent
    {
        public IViewComponentResult Invoke(GridOptions model)
        {
            return View(model);
        }

    }

    public class MatterSelectionGridOption : GridOptions
    {
        public int? KeyId { get; set; }
    }

}
