using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DataQueryDetailViewModel : DataQueryMain
    {
        public bool IsMyQuery { get; set; } = true;
        public List<string>? Tags { get; set; }
        [Display(Name = "Category")]
        public string? DQCat { get; set; }
    }
}
