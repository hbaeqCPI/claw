using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    // Shared flat DTOs that combine a base table with its _Ext sibling for display
    // in a single Kendo grid. `IsExt` drives the checkmark column on the right and
    // tells the grid which controller to route detail-link/delete actions to.

    public class DesCaseTypeSearchItem
    {
        [Display(Name = "Intl Code")]
        public string? IntlCode { get; set; }
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }
        [Display(Name = "Des Country")]
        public string? DesCountry { get; set; }
        [Display(Name = "Des Case Type")]
        public string? DesCaseType { get; set; }
        [Display(Name = "Default")]
        public bool Default { get; set; }
        // Nullable because only the _Ext table has a GenApp column; for base rows
        // it stays null, which naturally excludes them when the user filters on it.
        [Display(Name = "Gen App")]
        public bool? GenApp { get; set; }
        [Display(Name = "Systems")]
        public string? Systems { get; set; }
        [Display(Name = "_Ext")]
        public bool IsExt { get; set; }
    }

    public class DesCaseTypeDeleteSearchItem
    {
        [Display(Name = "Intl Code")]
        public string? IntlCode { get; set; }
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }
        [Display(Name = "Des Country")]
        public string? DesCountry { get; set; }
        [Display(Name = "Des Case Type")]
        public string? DesCaseType { get; set; }
        [Display(Name = "Default")]
        public bool Default { get; set; }
        [Display(Name = "Intl Code New")]
        public string? IntlCodeNew { get; set; }
        [Display(Name = "Case Type New")]
        public string? CaseTypeNew { get; set; }
        [Display(Name = "Des Country New")]
        public string? DesCountryNew { get; set; }
        [Display(Name = "Des Case Type New")]
        public string? DesCaseTypeNew { get; set; }
        [Display(Name = "Systems")]
        public string? Systems { get; set; }
        [Display(Name = "_Ext")]
        public bool IsExt { get; set; }
    }

    public class DesCaseTypeFieldsSearchItem
    {
        [Display(Name = "Des Case Type")]
        public string? DesCaseType { get; set; }
        [Display(Name = "From Field")]
        public string? FromField { get; set; }
        [Display(Name = "To Field")]
        public string? ToField { get; set; }
        // Nullable because only the _Ext table has InUse; base rows remain null.
        [Display(Name = "In Use")]
        public bool? InUse { get; set; }
        [Display(Name = "Systems")]
        public string? Systems { get; set; }
        [Display(Name = "_Ext")]
        public bool IsExt { get; set; }
    }
}
