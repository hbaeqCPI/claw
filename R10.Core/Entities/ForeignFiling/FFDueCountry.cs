using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ForeignFiling
{
    public class FFDueCountry : BaseEntity
    {
        [Key]
        public int DueCountryId { get; set; }

        [Required]
        public int DueId { get; set; }

        public string Source { get; set; }

        public string Country { get; set; }

        [Display(Name = "Gen App?")]
        public bool? GenApp { get; set; }

        [Display(Name = "Exclude?")]
        public bool? Exclude { get; set; } //do not process

        public int? GenId { get; set; } //id of generated countryApp/desCountry (Source==All ? AppId : DesId)

        public DateTime? UpdateDate { get; set; }

        public FFDue FFDue { get; set; }


        public PatCountry DesCountry { get; set; }
    }
}
