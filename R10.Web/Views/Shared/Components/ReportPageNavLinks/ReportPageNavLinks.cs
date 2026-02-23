using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ViewComponents
{ 
    public class ReportPageNavLinks : ViewComponent
    {
        public ReportPageNavLinks()
        {
        }

        public IViewComponentResult Invoke(ReportPageOption pageOption)
        {
            return View(pageOption);
        }
    }

    public class ReportPageOption
    {
        public bool CanSave { get; set; }
        public bool CanLoad { get; set; }
        public bool CanClear { get; set; }
        public bool CanPrint { get; set; }
        public bool CanEmail { get; set; }
        public string Url { get; set; }
    }
}