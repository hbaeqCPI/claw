using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMArea : BaseEntity
    {

        [Key]
        public int AreaID { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Area")]
        public string? Area { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public List<GMAreaCountry>? GMAreaCountries { get; set; }
    }
}
