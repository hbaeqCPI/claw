using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class UserSystemRoleViewModel //: CPiUserSystemRole
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [UIHint("UserSystem")]
        public PickListViewModel? System { get; set; }

        [UIHint("UserRole")]
        public PickListViewModel? Role { get; set; }

        [StringLength(10)]
        [UIHint("UserRespOffice")]
        public string? RespOffice { get; set; }
    }
}
