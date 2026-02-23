using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Identity
{
    public class CPiUserPasswordHistory
    {
        [StringLength(450)]
        public string UserId { get; set; }
        public string PasswordHash { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
    }
}
