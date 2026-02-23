using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class AttorneyListViewModel
    {
        public int AttorneyID { get; set; }
        
        [Display(Name = "Attorney")]
        public string? AttorneyCode { get; set; }
        
        [Display(Name = "Attorney Name")]
        public string? AttorneyName { get; set; }
    }
}
