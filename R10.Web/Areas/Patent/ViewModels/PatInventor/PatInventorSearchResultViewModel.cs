#nullable enable
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatInventorSearchResultViewModel
    {
        public int InventorID { get; set; }
        public string? Inventor { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }


        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string? MiddleInitial { get; set; }

        [Display(Name = "EMail")]
        public string? EMail { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }
    }

    public class DummySearchResultViewModel {
        
        public string? Field1 { get; set; }
        public string? Field2 { get; set; }
    }
}
