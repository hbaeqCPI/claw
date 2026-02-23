using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickEmailSetupRecipientViewModel : QERecipient
    {
        [Display(Name = "Role")]
        public string? RoleName { get; set; }
        public string? RoleType { get; set; }
        public string? Description { get; set; }
    }
}
