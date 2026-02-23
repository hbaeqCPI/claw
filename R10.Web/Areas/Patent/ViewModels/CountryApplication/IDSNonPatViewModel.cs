using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class IDSCopyNonPatSourceViewModel
    {
        
        public int NonPatLiteratureId { get; set; }

        [Display(Name = "Literature")]
        public string? NonPatLiteratureInfo { get; set; }

        public string? CaseNumber { get; set; }
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }
    }

    public class IDSNonPatLiteratureExportViewModel
    {
        [Display(Name = "Literature")]
        public string? NonPatLiteratureInfo { get; set; }

        [Display(Name = "Reference Source")]
        public string? ReferenceSrc { get; set; }

        [Display(Name = "Reference Date")]
        public DateTime? ReferenceDate { get; set; }

        [Display(Name = "IDS File Date")]
        public DateTime? RelatedDateFiled { get; set; }

        [Display(Name = "Applicable?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "Has Translation?")]
        public bool HasTranslation { get; set; }

        [Display(Name = "Saved Doc")]
        public string? CurrentDocFile { get; set; }

    }
}
