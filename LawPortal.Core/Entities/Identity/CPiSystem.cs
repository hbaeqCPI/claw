using LawPortal.Core.Entities.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Identity
{
    public class CPiSystem
    {
        [Key]
        [StringLength(450)]
        public String Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsRespOfficeOn { get; set; }

        public string? SystemType { get; set; }

        public List<CPiUserSystemRole>? UserSystemRoles { get; set; }
        public List<CPiUserTypeSystemRole>? UserTypeSystemRoles { get; set; }
        public List<CPiSSOClaimSystemRole>? SSOClaimSystemRoles { get; set; }
        public List<DocSystem>? DocSystems { get; set; }
    }
}
