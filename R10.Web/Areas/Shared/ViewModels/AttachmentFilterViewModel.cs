using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class AttachmentFilterViewModel:BaseEntity
    {
        public int ActId { get; set; }

        public string? Area { get; set; }
        public string? Controller { get; set; }

        public string[]? FileType { get; set; }

        [Display(Name = "Document Name(contains)")]
        public string? DocumentName { get; set; }

        public string[]? Tags { get; set; }
    }

    public class AttachmentFilterCriteriaViewModel
    {
        public string[]? FileType { get; set; }
        public string? DocumentName { get; set; }
        public string[]? Tags { get; set; }
    }
}
