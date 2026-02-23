using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class ReportParameter
    {
        [Key]
        [StringLength(450)]
        public string?  Id { get; set; }

        public string?  Parameters { get; set; }
    }
}
