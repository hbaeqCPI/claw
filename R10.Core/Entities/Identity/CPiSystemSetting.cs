using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Identity
{
    public class CPiSystemSetting
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string SystemId { get; set; }

        [Required]
        public int SettingId { get; set; }

        public string Settings { get; set; }

        public CPiSetting CPiSetting { get; set; }
    }
}
