using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Identity
{
    public class CPiUserEntityFilter
    {
        [StringLength(450)]
        public string UserId { get; set; }

        public int EntityId { get; set; }

        public CPiUser CPiUser { get; set; }
    }
}
