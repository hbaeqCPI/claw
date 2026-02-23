using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Models.DocumentViewModels;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.ViewComponents
{
    public class Documents : ViewComponent
    {
        public IViewComponentResult Invoke(MainDocumentOptions model)
        {
            return View(model);
        }

    }

    public class MainDocumentOptions {
        public string? SystemType { get; set; }
        public string? ParentKey { get; set; }
        public string? ScreenCode { get; set; }
        public int ParentValue { get; set; }
        public string? RoleLink { get; set; }

        public ChildImageViewModel? ChildImageViewModel { get; set; }
        public DocumentFileHistoryViewModel? FileHistoryViewModel { get; set; }
    } 

}


