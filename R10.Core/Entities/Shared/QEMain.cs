using R10.Core.Entities.Patent;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class QEMain: BaseEntity
    {
        [Key]
        public int QESetupID { get; set; }

        [Required(ErrorMessage = "Screen Name is required.")]
        [Display(Name = "Screen Name")]
        public int ScreenId { get; set; }

        [Required(ErrorMessage = "Data Source is required.")]
        [Display(Name = "Data Source")]
        public int DataSourceID { get; set; }

        [Required(ErrorMessage = "Template Name is required.")]
        [Display(Name = "Template Name")]
        [StringLength(50)]
        public string?  TemplateName { get; set; }
        
        public string?  Remarks { get; set; }

        [Display(Name = "Default?")]
        public bool IsDefault { get; set; }

        [Display(Name = "In Use?")]
        public bool InUse { get; set; }

        [Display(Name = "CPI Template?")]
        public bool CPITemplate { get; set; }

        [StringLength(500)]
        [Display(Name = "Subject")]
        public string?  Subject { get; set; }
        public string?  Detail { get; set; }
        public string?  Header { get; set; }
        public string?  Footer { get; set; }

        public bool FromUseSender { get; set; }
        [StringLength(500)]
        public string?  FromAddress { get; set; }
        public bool ReplyToUseSender { get; set; }
        [StringLength(500)]
        public string?  ReplyToAddress { get; set; }

        [StringLength(100)]
        [Display(Name = "Language")]
        public string?  Language { get; set; }

        [StringLength(200)]
        public string?  FilePath { get; set; }

        [StringLength(5)]
        public string?  FileExt { get; set; }

        [StringLength(20)]
        public string?  FilePrefix { get; set; }

        public SystemScreen? SystemScreen { get; set; }

        public QEDataSource? DataSource { get; set; }

        public Language? LanguageLookup { get; set; }
        public List<PatSearchNotify>? PatSearchsNotify { get; set; }
        public List<LetterMain>? LettersForSignature { get; set; }
        public List<EFS>? EFSForSignature { get; set; }

        [Display(Name = "Category")]
        public int? QECatId { get; set; }

        public List<QETag>? QETags { get; set; }
        public QECategory? QECategory { get; set; }
    }
}
