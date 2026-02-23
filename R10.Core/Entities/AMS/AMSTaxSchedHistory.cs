using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSTaxSchedHistory
    {
        [Key]
        public int LogID { get; set; }
        public int AnnID { get; set; }

        [StringLength(5)]
        [Display(Name = "CPI Tax Schedule")]
        public string? CPITaxSchedule { get; set; }

        [StringLength(500)]
        [Display(Name = "Reason For Change")]
        public string? ReasonForChange { get; set; }

        [Display(Name = "Date Changed")]
        public DateTime DateChanged { get; set; }

        [StringLength(20)]
        [Display(Name = "Changed By")]
        public string? CreatedBy { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }
    }
}
