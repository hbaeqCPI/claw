using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ClientDesignatedCountryViewModel : BaseEntity
    {
        public int EntityDesCtryID { get; set; }
        public int ParentDesCtryID { get; set; }

        [Display(Name = "Gen App?")]
        public bool GenApp { get; set; }

        public int ClientID { get; set; }
        public bool ReadOnly { get; set; }

        public string? SystemType {get; set; }
         
        [Display(Name = "System Type")]
        public string? SystemTypeName {get; set; }

        [Display(Name = "Country")]
        [Required]
        public string? DesCtry { get; set; }

        [Display(Name = "Country Name")]
        public string? DesCtryName { get; set; }

        [Display(Name = "Case Type")]
        [Required]
        public string? DesCaseType { get; set; }
    }

    public class DesCountryLookupViewModel
    {
        public int CountryID { get; set; }

        [Required(ErrorMessage="Country field is required")]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }
    }
}

