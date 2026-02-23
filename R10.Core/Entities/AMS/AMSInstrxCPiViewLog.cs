using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.AMS
{
    public class AMSInstrxCPiViewLog
    {

        [Key]
        public int Id { get; set; }

        [StringLength(256)]
        [Required]
        public string? UserName { get; set; }

        public DateTime? ViewDate { get; set; }

        public DateTime? ReminderDate { get; set; }
    }
}
