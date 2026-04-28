using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace LawPortal.Core.Identity
{
    public class CPiRole : IdentityRole
    {
        public bool IsEnabled { get; set; }
        public bool CanModify
        {
            get => (
                    Id.ToLower() == "modify" || 
                    Id.ToLower() == "remarksonly" || 
                    Id.ToLower() == "nodelete"
                    );
        }
        public List<CPiUserSystemRole> UserSystemRoles { get; set; }
    }
}
