// using R10.Core.Entities.DMS; // Removed during deep clean
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

//         public List<Disclosure>? AreaDisclosures { get; set; } // Removed during deep clean
//         public List<DMSEntityReviewer>? Reviewers { get; set; } // Removed during deep clean
//         public List<DMSAgenda>? AreaDMSAgendas { get; set; } // Removed during deep clean
    }
}
