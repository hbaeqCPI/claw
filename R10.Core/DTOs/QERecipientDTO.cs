using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class QERecipientRoleDTO
    {
        public int TableLevel { get; set; }
        public string? ParentTable { get; set; }
        public int ParentID { get; set; }
        public string? RoleType { get; set; }
        public int RoleClass { get; set; }

        [Display(Name = "Type")]
        public string? EntityTitle { get; set; }

        [Display(Name = "Code")]
        public string? EntityCode { get; set; }

        [Display(Name = "Name")]
        public string? EntityFullName { get; set; }

        [Display(Name = "Email")]
        public string? EntityEmail { get; set; }

    }
}
