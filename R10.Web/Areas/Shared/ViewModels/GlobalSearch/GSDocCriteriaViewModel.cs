using R10.Core.DTOs;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class GSDocCriteriaViewModel : GSDocFilterBase
    {
        [Display(Name = "Document")]
        [UIHint("GSDocFieldDropDown")]
        public GSFieldListViewModel? Field { get; set; }
    }
}
