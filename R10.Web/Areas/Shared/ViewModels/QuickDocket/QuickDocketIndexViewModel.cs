
using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickDocketIndexViewModel
    {
        public DetailPagePermission? PagePermission { get; set; }
        public QuickDocketSearchCriteriaViewModel? SearchCriteriaViewModel { get; set; }
        public QuickDocketDefaultSettingsViewModel? DefaultSettingsViewModel { get; set; }


    }

    public class QuickDocketMassUpdateViewModel
    {
        public string? DateType { get; set; }
        public DateTime? SpecificDate { get; set; }
    }

    public class QuickDocketDeDocketInstructionMassUpdateViewModel
    {
        public string? Instruction { get; set; }
        public string? Remarks { get; set; }

        [Display(Name = "Empty instructions only?")]
        public bool EmptyInstructionOnly { get; set; } = true;
        public string? SystemTypes { get; set; }
        public string? Indicators { get; set; }
    }
}
