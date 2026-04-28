using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Identity
{
    public class CPiSystemRole
    {
        [StringLength(450)]
        public string SystemId { get; set; }

        [StringLength(450)]
        public string RoleId { get; set; }
    }
}
