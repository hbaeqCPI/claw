using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LawPortal.Core.Entities
{
    public class ModuleMain: BaseEntity
    {
        [Key]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(10)]
        public string?  ModuleCode { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Sub Module")]
        public string?  SubModule { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Module Name")]
        public string?  ModuleName { get; set; }

        [Required]
        [Display(Name = "System Type")]
        [StringLength(1)]
        public string?  SystemType { get; set; }


        [Required]
        [StringLength(10)]
        public string?  DocClass { get; set; }
        
        //[Display(Name = "Letter Category")]
        //public int LtrCateg { get; set; }

        [StringLength(255)]
        public string?  QEImageRecSource { get; set; }

        [StringLength(50)]
        public string?  QEScreenKey { get; set; }

        [StringLength(50)]
        public string?  QELogRecSource { get; set; }

        //[StringLength(50)]
        //public string?  ScreenName { get; set; }

    }
}
