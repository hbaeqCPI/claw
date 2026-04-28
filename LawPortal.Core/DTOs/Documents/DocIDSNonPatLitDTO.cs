using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.DTOs
{
    [Keyless]
    public class DocIDSNonPatLitDTO
    {
        public int NonPatLiteratureId { get; set; }

        [Display(Name="Literature")]
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

        public string? DocUrl { get; set; }
        public string? DocFileName { get; set; }

        [NotMapped]
        public string? DocumentLink { get; set; }
    }
}
