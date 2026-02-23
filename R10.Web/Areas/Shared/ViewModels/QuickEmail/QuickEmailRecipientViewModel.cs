using R10.Core.DTOs;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickEmailRecipientRoleViewModel : QERecipientRoleDTO
    {
        [Display(Name="Send As")]
        public string? SendAs { get; set; }

        [Display(Name = "Default?")]
        public bool IsDefault { get; set; }
    }
}
