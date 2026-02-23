using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Components
{
    public class SingleMultiSwitcher : ViewComponent
    {
        public SingleMultiSwitcher()
        {
        }

        public IViewComponentResult Invoke(string MultiClass, string SingleClass)
        {
            return View(new SingleMultiSwitcherOption() { MultiClass=MultiClass,SingleClass=SingleClass});
        }
    }

    public class SingleMultiSwitcherOption
    {
        public string MultiClass { get; set; }
        public string SingleClass { get; set; }
    }
}