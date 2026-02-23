using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.RMS
{
    public class RMSReminderSetupDoc : BaseEntity
    {
        [Key]
        public int SetUpDocId { get; set; }

        [Required]
        public int SetupId { get; set; }

        [Required]
        public int DocId { get; set; }


        public RMSReminderSetup? RMSReminderSetup { get; set; }
        public RMSDoc? RMSDoc { get; set; }
    }
}
