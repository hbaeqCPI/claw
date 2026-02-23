using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class FormIFWActionMapViewModel : BaseEntity
    {
        //[Key]
        public int MapId { get; set; }
        
        public int MapHdrId { get; set; }
        public int DocTypeId { get; set; }

        [Display(Name = "Auto-Generate Action?")]
        public bool IsGenActionDE { get; set; }

        [Display(Name = "Compare Action?")]
        public bool IsCompareDE { get; set; }

        [Display(Name = "IFW Document Type")]
        public string? DocDesc { get; set; }

        public string? SystemType { get; set; }

        [Display(Name = "Docket Required?")]
        public bool IsActRequired { get; set; }

        [Display(Name = "Check Docket?")]
        public bool CheckAct { get; set; }

        [Display(Name = "Forward document to client?")]
        public bool SendToClient { get; set; }

        [Display(Name = "Used in AI?")]
        public bool UseAI { get; set; } = true;

        [Display(Name = "Watch?")]
        public bool UseWatch { get; set; }

    }
}
