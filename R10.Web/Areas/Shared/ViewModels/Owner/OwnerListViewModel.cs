using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class OwnerListViewModel
    {
        public int OwnerID { get; set; }

        public string? OwnerCode { get; set; }

        [Display(Name = "Owner Name")]
        public string? OwnerName { get; set; }
    }
}
