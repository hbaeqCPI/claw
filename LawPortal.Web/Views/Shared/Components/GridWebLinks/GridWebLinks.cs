using Microsoft.AspNetCore.Mvc;

namespace LawPortal.Web.ViewComponents
{
    public class GridWebLinks : ViewComponent
    {
        public IViewComponentResult Invoke(WebLinksGridOptions model)
        {
            return View(model);
        }
    }

    public class WebLinksGridOptions
    {
        public int Id { get; set; }
        public string Module { get; set; }
        public string Controller { get; set; }
    }
}
