using R10.Web.Helpers;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickDocketExportViewModel
    {
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        [Display(Name = "LabelCaseNumber")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "LabelClient")]
        public string? Client { get; set; }

        [Display(Name = "LabelAttorney1")]
        public string? Attorney1 { get; set; }

        [Display(Name = "LabelAttorney2")]
        public string? Attorney2 { get; set; }

        [Display(Name = "LabelAttorney3")]
        public string? Attorney3 { get; set; }

        [Display(Name = "LabelAttorney4")]
        public string? Attorney4 { get; set; }

        [Display(Name = "LabelAttorney5")]
        public string? Attorney5 { get; set; }

        [Display(Name = "Resp")]
        public string? Responsible { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [NoExport]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Filing Date")]
        public string? FilDateString { get; set; }

        [Display(Name = "System")]
        public string? System { get; set; }

        [Display(Name = "LabelOwner")]
        public string? Owner { get; set; }

        [Display(Name = "LabelClientRef")]
        public string? ClientRef { get; set; }

        [Display(Name = "Patent/Reg No.")]
        public string? PatRegNumber { get; set; }

        [NoExport]
        public DateTime? IssRegDate { get; set; }
        
        [Display(Name = "Issue/Reg Date")]
        public string? IssRegDateString { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Remarks")]
        public string? DueDateRemarks { get; set; }



        [Display(Name = "Instruction")]
        public string Instruction { get; set; }

        [Display(Name = "Instruction Remarks")]
        public string DeDocketRemarks { get; set; }

        [Display(Name = "Instructed By")]
        public string InstructedBy { get; set; }

        [NoExport]
        public DateTime? InstructionDate { get; set; }

        [Display(Name = "Instruction Date")]
        public string InstructionDateString { get; set; }

        [NoExport]
        public bool? InstructionCompleted { get; set; }

        [Display(Name = "Completed")]
        public string InstructionCompletedString { get; set; }

    }

}
