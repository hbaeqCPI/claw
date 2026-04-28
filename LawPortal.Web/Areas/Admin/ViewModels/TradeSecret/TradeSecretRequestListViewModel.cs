using LawPortal.Core.Entities.Shared;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Areas.Admin.ViewModels
{
    public class TradeSecretRequestListViewModel
    {
        public int RequestId { get; set; }

        public string? Email { get; set; }

        [Display(Name = "Screen")]
        public string? ScreenId { get; set; }

        public int RecId { get; set; }

        public string? Status { get; set; }

        public DateTime? RequestDate { get; set; }

        [Display(Name = "Expired?")]
        public bool IsExpired { get; set; }
    }
}
