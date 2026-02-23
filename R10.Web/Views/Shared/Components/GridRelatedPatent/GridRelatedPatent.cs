using Microsoft.AspNetCore.Mvc;
using R10.Web.Models;

namespace R10.Web.ViewComponents
{
    public class GridRelatedPatent : ViewComponent
    {
        public IViewComponentResult Invoke(GridOptions model)
        {
            return View(model);
        }

    }

    public class PatentSelectionGridOption : GridOptions
    {
        public int? KeyId { get; set; }
        public bool IncludeInvention { get; set; } = false;
        public bool InventionOnly { get; set; } = false;
    }

}
