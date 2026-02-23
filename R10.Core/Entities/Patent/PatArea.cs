using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{

    public class PatArea:BaseEntity
    {
    
        [Key]
        public int AreaID { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Area")]
        public string Area { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        public List <PatAreaCountry>? PatAreaCountries { get; set; }

        public List<Disclosure>? AreaDisclosures { get; set; }
        public List<DMSEntityReviewer>? Reviewers { get; set; }
        public List<DMSAgenda>? AreaDMSAgendas { get; set; }
    }
}
