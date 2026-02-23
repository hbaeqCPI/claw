using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatIDSNonPatLiterature : BaseEntity
    {
        [Key]
        public int NonPatLiteratureId { get; set; }

        public int? AppId { get; set; }

        [Display(Name="Literature")]
        public string? NonPatLiteratureInfo { get; set; }

        [StringLength(255)]
        [Display(Name = "New Document")]
        public string? DocFilePath { get; set; }

        [Display(Name = "IDS File Date")]
        public DateTime? RelatedDateFiled { get; set; }

        [Display(Name = "Reference Date")]
        public DateTime? ReferenceDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Reference Source")]
        public string? ReferenceSrc { get; set; }

        [Display(Name = "Applicable?")]
        public bool ActiveSwitch { get; set; }

        public int? FileId { get; set; }

        [Display(Name = "Has Translation?")]
        public bool HasTranslation { get; set; }

        public string? DataSource { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Saved Doc")]
        [NotMapped]
        public string? CurrentDocFile { get; set; }
        public DateTime? MetaUpdate { get; set; } = DateTime.Now;           // Azure blob storage metadata update date

        [NotMapped]
        public bool WithFee{ get; set; }

        [Display(Name = "Considered By Examiner?")]
        public bool ConsideredByExaminer { get; set; }

        public CountryApplication? CountryApplication { get; set; }
    }
}



