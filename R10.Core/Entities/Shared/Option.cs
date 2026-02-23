using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Shared
{
    public partial class Option : BaseEntity
    {
        [Key]
        public int OptionID { get; set; }

        [StringLength(50)]
        public string? OptionKey { get; set; }

        [StringLength(50)]
        public string? OptionSubKey { get; set; }
        
        public string? OptionValue { get; set; }
        public string? OptionRemarks { get; set; }

    }
}
