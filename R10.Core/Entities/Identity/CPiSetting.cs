using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Identity
{
    public class CPiSetting
    {
        [Key]
        public int Id { get; set; }

        [StringLength(256)]
        [Required]
        public string Name { get; set; }

        [StringLength(450)]
        public string Policy { get; set; }
    }
}
