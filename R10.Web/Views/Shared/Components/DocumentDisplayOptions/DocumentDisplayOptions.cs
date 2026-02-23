using Microsoft.AspNetCore.Mvc;
using R10.Web.ViewComponents;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.ViewComponents
{
    public class DocumentDisplayOptions : ViewComponent
    {
        public IViewComponentResult Invoke(DocumentDisplayOption? defaultOption)
        {
            return View(defaultOption);
        }

    }

    public class DocumentDisplayOptionListItem
    {
        public string? Text { get; set; }
        public DocumentDisplayOption? Value { get; set; }
        public string? Icon { get; set; }
    }

    public enum DocumentDisplayOption
    {
        [Display(Name = "fa-solid fa-table", Description = "Grid View")]
        GridView,
        [Display(Name = "fa-solid fa-grid", Description = "Gallery View")]
        GalleryView
    }
}
