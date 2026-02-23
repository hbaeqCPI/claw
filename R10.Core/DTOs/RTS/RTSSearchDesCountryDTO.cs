using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchDesCountryDTO
    {
        public int PLAppId { get; set; }

        [Display(Name= "Country")]
        public string? DesCountry { get; set; }

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "CaseType")]
        public string? MapCaseType { get; set; }
    }
}
