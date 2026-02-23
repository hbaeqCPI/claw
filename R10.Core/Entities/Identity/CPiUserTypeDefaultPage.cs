using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Identity
{
    public class CPiUserTypeDefaultPage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public CPiUserType UserType { get; set; }

        [Required]
        public int DefaultPageId { get; set; }
    }
}
