using Microsoft.AspNetCore.Mvc;

namespace R10.Web.ViewComponents
{
    public class InitialButtonGroup : ViewComponent
    {
        public IViewComponentResult Invoke(InitialButtonGroupOptions model)
        {
            return View("Default", model);
        }
    }

    public class InitialButtonGroupOptions
    {
        public string? Name { get; set; }
        public string? FormId { get; set; }
    }
}
