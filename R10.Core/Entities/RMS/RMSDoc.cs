using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.RMS
{
    public class RMSDoc : BaseEntity
    {
        [Key]
        public int DocId { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Enabled")]
        public bool IsEnabled { get; set; } //do not allow disabling if doc is used and not uploaded yet


        public List<RMSDueDoc>? RMSDueDocs { get; set; }
        public List<RMSReminderSetupDoc>? RMSReminderSetupDocs { get; set; }
    }
}
