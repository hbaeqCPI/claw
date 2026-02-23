using Microsoft.AspNetCore.Mvc;
using R10.Web.Models;

namespace R10.Web.ViewComponents
{
    public class GridRelatedTrademark : ViewComponent
    {
        public IViewComponentResult Invoke(GridOptions model)
        {
            return View(model);
        }

    }

    public class TrademarkSelectionGridOption : GridOptions
    {
        public int? KeyId { get; set; }
    }
}
