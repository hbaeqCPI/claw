using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class DOCXUserData : DOCXUserDataDetail
    {
        public DOCXMain? DOCXMain { get; set; }
    }
    public class DOCXUserDataDetail : BaseEntity
    {
        [Key]
        public int DOCXDataId { get; set; }

        public int DOCXId { get; set; }

        [Required]
        [Display(Name = "Data Name")]
        [StringLength(50)]
        public string?  DataName { get; set; }

        [Display(Name = "Default Value")]
        [StringLength(50)]
        public string?  DefaultValue { get; set; }
    }
}
