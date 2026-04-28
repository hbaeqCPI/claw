using LawPortal.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class WorkflowHeaderViewModel
    {
        public int Id { get; set; }
        public bool SendEmail { get; set; }
        public string? EmailUrl { get; set; }
        public List<WorkflowViewModel>? Workflows { get; set; }

    }

    public class WorkflowViewModel
    {
        public int TriggerValueId { get; set; }
        public int ActionTypeId { get; set; }
        public int ActionValueId { get; set; }
        public bool Preview { get; set; }
        public bool AutoAttachImages { get; set; }
        public List<WorkflowEmailAttachmentViewModel>? Attachments { get; set; }
        public string? EmailUrl { get; set; }
        public int? Id { get; set; }
        public string? EmailTo { get; set; }
        public string? AttachmentFilter { get; set; }
    }

    public class WorkflowEmailAttachmentViewModel
    {
        public int DocId { get; set; }
        public string? FileName { get; set; }
        public string? OrigFileName { get; set; }
        public int DocParent { get; set; }
        public int? FileId { get; set; }
        public string? Id { get; set; }
        public DateTime? DocDate { get; set; }
    }

    public class WorkflowEmailViewModel
    {
        public bool isAutoEmail { get; set; }
        public int qeSetupId { get; set; }
        public bool autoAttachImages { get; set; }
        public int id { get; set; }
        public string[]? fileNames { get; set; }
        public int parentId { get; set; }
        public string? emailUrl { get; set; }
        public string? emailTo { get; set; }
        public string? strId { get; set; }
        public string? attachmentFilter { get; set; }
    }

    public class WorkflowSignatureViewModel
    {
        public int QESetupId { get; set; }
        public int ParentId { get; set; }
        //public Dictionary<string,string> UserFileNames { get; set; }
        public WorkflowSignatureDocViewModel UserFile { get; set; }
        public string? ScreenCode { get; set; }
        public string? SystemTypeCode { get; set; }
        public string? RoleLink { get; set; }

        public string? SharePointDocLibrary { get; set; }

        
    }

    public class WorkflowSignatureDocViewModel
    {
        public string? Name { get; set; }
        public string? FileName { get; set; }
        public int? FileId { get; set; }
        public string? StrId { get; set; }
    }
}
