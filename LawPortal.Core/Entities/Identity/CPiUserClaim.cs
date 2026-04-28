using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LawPortal.Core.Identity
{
    [NotMapped]
    public class CPiUserClaim : IdentityUserClaim<string>
    {
        public CPiUser CPiUser { get; set; }
    }
}
