using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class AttorneySearchResultViewModel
    {
        public int AttorneyID { get; set; }

        [Display(Name = "Attorney")]
        public string? AttorneyCode { get; set; }

        [Display(Name = "Attorney Name")]
        public string? AttorneyName { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }
    }
}
