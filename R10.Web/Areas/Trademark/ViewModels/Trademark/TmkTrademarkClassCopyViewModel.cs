using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkTrademarkClassCopyOptionViewModel
    {
        public int ParentTmkId { get; set; }
        public int TmkId { get; set; }
    }

    public class TmkTrademarkClassCopyViewModel 
    {
        public int TmkClassId { get; set; }

        [Display(Name ="Class")]
        public string? ClassDesc { get; set; }

        [Display(Name = "Goods")]
        public string? Goods { get; set; }
    }
}
