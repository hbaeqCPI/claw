using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class AmsInstructionsToCpiExportViewModel: AmsCaseListExportViewModel
    {
        [Display(Name = "Instruction")]
        public string? Instruction { get; set; }

        [Display(Name = "Instruction Date")]
        public DateTime? InstructionDate { get; set; }
    }

    public class AmsCostSavingDrillDownViewModel : AmsCostSavingExportViewModel
    {
        public int Id { get; set; }
    }

    public class AmsCostSavingExportViewModel : AmsCaseListExportViewModel
    {
        [Display(Name = "Instruction")]
        public string? ClientInstruction { get; set; }

        [Display(Name = "CPI Instruction")]
        public string? CPIInstruction { get; set; }
    }

    public class AmsCaseListDrillDownViewModel : AmsCaseListExportViewModel
    {
        public int Id { get; set; }
    }

    public class AmsCaseListExportViewModel : AmsMainCaseListExportViewModel
    {
        [Display(Name = "No")]
        public string? AnnuityNoDue { get; set; }

        [Display(Name = "Annuity Due Date")]
        public DateTime? AnnuityDueDate { get; set; }

        [Display(Name = "Annuity Cost")]
        public decimal AnnuityCost { get; set; }        
    }    

    public class AmsMainCaseListExportViewModel
    {
        [Display(Name = "Case Number")]
        public string CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Sub Case")]
        public string SubCase { get; set; }

        [StringLength(10)]
        public string? CPIClient { get; set; }

        [Display(Name = "Case Type")]
        public string CPICaseType { get; set; }

        [Display(Name = "Status")]
        public string CPIStatus { get; set; }

        [Display(Name = "Title")]
        public string CPITitle { get; set; }

        [Display(Name = "Application No.")]
        public string CPIAppNo { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? CPIFilDate { get; set; }

        [Display(Name = "Publication No.")]
        public string CPIPubNo { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? CPIPubDate { get; set; }

        [Display(Name = "Patent No.")]
        public string CPIPatNo { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? CPIIssDate { get; set; }


        [Display(Name = "Expiration Date")]
        public DateTime? CPIExpireDate { get; set; }
                
        [Display(Name = "Annuity Code")]
        public string CPIClientCode { get; set; }
    }    
}
