using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using R10.Core.Entities.Shared;

namespace R10.Core.Entities
{
    public class LetterMain : LetterMainDetail
    {
        public LetterCategory? LetterCategory { get; set; }
        public LetterSubCategory? LetterSubCategory { get; set; }
        public SystemScreen? SystemScreen { get; set; }
        public List<LetterRecordSource>? LetterRecordSources { get; set; }
        public List<LetterUserData>? LetterUserData { get; set; }
        public QEMain? QEMain { get; set; }
        public List<LetterTag>? LetterTags { get; set; }
    }

    public class LetterMainDetail : BaseEntity
    {
        [Key]
        public int LetId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Letter Name")]
        public string?  LetName { get; set; }

        [Required]
        [Display(Name = "Screen")]
        public int ScreenId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int LetCatId { get; set; }

        [Required]
        [Display(Name = "Template File")]
        [StringLength(100)]
        public string?  TemplateFile { get; set; }

        [Display(Name = "CPI Template?")]
        public bool IsCPiTemplate { get; set; }

        [Display(Name = "Has Image?")]
        public bool HasImage { get; set; }

        //[Display(Name = "Image Size")]                // image size should be specified in the template file
        //public int ThumbnailSize { get; set; }

        [Display(Name = "Remarks")]
        public string?  Remarks { get; set; }

        [Display(Name = "eSignature Needed?")]
        public bool ForSignature { get; set; }

        public int? SignatureQESetupId { get; set; }

        [Display(Name = "Sub Category")]
        public int? LetSubCatId { get; set; }

    }
}
