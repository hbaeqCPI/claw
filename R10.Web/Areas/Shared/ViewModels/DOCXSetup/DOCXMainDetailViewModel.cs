using System.ComponentModel.DataAnnotations;
using R10.Core.Entities;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DOCXMainDetailViewModel : DOCXMainDetail
    {
        [Display(Name = "Screen")]

        public string? ScreenName { get; set; }


        [Display(Name = "Category")]
        public string? DOCXCatDesc { get; set; }

        public string? SystemType { get; set; }
    }
}
