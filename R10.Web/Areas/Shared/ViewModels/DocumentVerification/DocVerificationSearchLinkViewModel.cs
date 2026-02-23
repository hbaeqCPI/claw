using R10.Core.DTOs;
using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocVerificationSearchLinkViewModel : GSSearchDTO
    {
        public bool IsActRequired { get; set; }
        public string? RespDocketing { get; set; }

        public string? RespReporting { get; set; }

        [Display(Name = "Responsible (Docketing)")]
        public string[]? RespDocketings { get; set; }

        [Display(Name = "Responsible (Reporting)")]
        public string[]? RespReportings { get; set; }
    }

    public class DocVerificationSearchFieldViewModel
    {
        public int KeyId { get; set; }
        public int EntryOrder { get; set; }
        public string? System { get; set; }
        public string? FieldLabel { get; set; }
        public bool IsEnabled { get; set; }
    }
}
