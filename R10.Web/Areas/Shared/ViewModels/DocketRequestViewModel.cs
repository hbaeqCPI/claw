using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocketRequestViewModel
    {
        
        public string? Area { get; set; }
        public string? Controller { get; set; } = "ActionDue";

        public string? SystemType { get; set; }
        public int ParentId { get; set; }
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name ="Request Type")]
        public string? RequestType { get; set; } = "New Docket";
        public int[]? ResponsibleId { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        public bool HasCountry { get; set; } = true;
        public bool HasSubCase { get; set; } = true;
        public bool HasCaseType { get; set; } = true;
        public bool HasStatus { get; set; } = true;

        public string? DefaultResponsible { get; set; }
        public int? DefaultResponsibleId { get; set; }

        public string? DocFile { get; set; }
        public int? FileId { get; set; }
        public string? DriveItemId { get; set; }

        public bool LoadScreen { get; set; }
        public string? RefreshCountUrl { get; set; }
    }
}
