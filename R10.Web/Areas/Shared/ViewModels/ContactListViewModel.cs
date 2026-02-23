using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ContactListViewModel
    {
        public int ContactID { get; set; }
        public string? Contact { get; set; }
        public string? ContactName { get; set; }
    }

    public class ContactSearchResultViewModel
    {
        public int ContactID { get; set; }

        [Display(Name ="Contact Code")]
        public string? Contact { get; set; }

        [Display(Name = "Contact Name")]
        public string? ContactName { get; set; }
    }

}
