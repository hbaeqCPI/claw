using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using R10.Core.Entities.Shared;

namespace R10.Core.Entities
{
    public class DOCXMain : DOCXMainDetail
    {
        public DOCXCategory? DOCXCategory { get; set; }
        public SystemScreen? SystemScreen { get; set; }
        public List<DOCXRecordSource>? DOCXRecordSources { get; set; }
        public List<DOCXUserData>? DOCXUserData { get; set; }
    }

    public class DOCXMainDetail : BaseEntity
    {
        [Key]
        public int DOCXId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "DOCX Name")]
        public string?  DOCXName { get; set; }

        [Required]
        [Display(Name = "Screen")]
        public int ScreenId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int DOCXCatId { get; set; }

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
    }
}
