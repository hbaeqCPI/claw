using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;

namespace R10.Web.ViewComponents
{
    public class GridWebLinks : ViewComponent
    {
        public IViewComponentResult Invoke(int parentId, string module, string subModule,string subSystem)
        {
            var model = new WebLinksGridOptions
            {
                ParentId = parentId,
                Module = module,
                SubModule=string.IsNullOrEmpty(subModule) ? "FormLink":subModule,
                SubSystem=subSystem
            };
            return View(model);
        }

    }
    public class WebLinksGridOptions
    {
        public int ParentId { get; set; }
        public string Module { get; set; }
        public string SubModule { get; set; }
        public string SubSystem { get; set; }
    }

    public enum WebLinksDisplayOption { RecordLink = 0, Search,All }
}
