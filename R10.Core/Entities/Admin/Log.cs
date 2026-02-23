using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        public string? Message { get; set; }

        public string? MessageTemplate { get; set; }

        [StringLength(128)]
        public string? Level { get; set; }

        public string? Properties { get; set; }

        [Required]
        public DateTime TimeStamp { get; set; }

        public string? Exception { get; set; }

        public string? LogEvent { get; set; }
        //public string? RequestForm { get; set; }
    }
}
