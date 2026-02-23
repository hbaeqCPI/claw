using R10.Web.Helpers;
using System.ComponentModel.DataAnnotations;
using static R10.Web.Helpers.ImageHelper;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocVerificationRequestDocZoomViewModel
    {
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }
        public int ParentId { get; set; }
        public int InvId { get; set; }

        //Parent
        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }
        public string? Status { get; set; }
        [Display(Name = "Application Number")]
        public string? AppNumber { get; set; }
        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }
        public string? RespOffice { get; set; }

        //DocketRequest
        [Display(Name = "Request Type")]
        public string? RequestType { get; set; }
        [Display(Name = "Requested On")]
        public DateTime? DateCreated { get; set; }
        [Display(Name = "Requested By")]
        public string? CreatedBy { get; set; }        
        

        //DeDocket
        public int ActId { get; set; }
        [Display(Name = "Action Type")]
        public string?  ActionType { get; set; }
        [Display(Name = "Action Due")]
        public string?  ActionDue { get; set; }
        [Display(Name = "Base Date")]
        public DateTime? BaseDate { get; set; }
        public string?  Indicator { get; set; }
        [Display(Name = "Instruction")]
        public string?  Instruction { get; set; }
        [Display(Name = "Instructed By")]
        public string?  InstructedBy { get; set; }
        [Display(Name = "Instruction Date")]
        public DateTime? InstructionDate { get; set; }
        [Display(Name = "Instruction Completed")]
        public bool InstructionCompleted { get; set; }

        //Shared
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }
        [Display(Name = "Completed By")]
        public string?  CompletedBy { get; set; }
        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }
        public int? FileId { get; set; }
        public string? DocFile { get; set; }
        public string? DriveItemId { get; set; }
        public string? Remarks { get; set; }

        public bool CanSave { get; set; }
        
        //Document
        public string? DocName { get; set; }
        public string? System { get; set; }
        public string? ScreenCode { get; set; }
        public string? DocFileName { get; set; }
        public CPiSavedFileType FileType { get; set; }
    }
}