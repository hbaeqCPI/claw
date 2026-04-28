using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LawPortal.Core.Identity
{
    public class CPiUserSettingLog
    {
        [Key]
        public int LogId { get; set; }

        [StringLength(450)]
        [Required]
        public string UserId { get; set; }

        [Required]
        public string SettingName { get; set; }

        public string? OldValue { get; set; }
        
        [Required]
        public string NewValue { get; set; }

        public DateTime ChangeDate{ get; set; }
    }
}
