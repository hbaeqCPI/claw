using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Components
{
    public class RMultiSelectClient : ViewComponent
    {
        public RMultiSelectClient()
        {
        }

        public IViewComponentResult Invoke(ReportPartialLookUpViewModel viewModel)
        {
            return View(viewModel);
        }
    }
}