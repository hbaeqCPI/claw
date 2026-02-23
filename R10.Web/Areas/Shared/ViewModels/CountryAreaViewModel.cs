using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CountryAreaViewModel
    {
        public int AreaCtryId { get; set; }

        [Required]
        [UIHint("Country")]
        [Display(Name ="Country")]
        public string? Country { get; set; }

        [Required(ErrorMessage = "Area is required.")]
        [Display(Name = "Area")]
        public int AreaID { get; set; }
      
        public string? Area { get; set; }

        [Display(Name = "Description")]
        public string? AreaDescription { get; set; }

        public CountryLookupViewModel? CountryLookup { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
        public byte[]? tStamp { get; set; }
    }

}
