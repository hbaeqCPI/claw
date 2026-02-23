using R10.Core.DTOs;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class GSDataCriteriaViewModel : GSDataFilterBase
    {
        [Display(Name="Field")]
        [UIHint("GSDataFieldDropDown")]
        public GSFieldListViewModel? Field { get; set; }
    }
}
