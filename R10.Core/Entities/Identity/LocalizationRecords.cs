using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class LocalizationRecords
    {
        [Key]
        public long Id { get; set; }

        [StringLength(450)]
        [Required]
        public string Key { get; set; }

        [StringLength(100)]
        [Required]
        public string LocalizationCulture { get; set; }

        [StringLength(450)]
        [Required]
        public string ResourceKey { get; set; }

        //[Required]
        public string? Text { get; set; }

        public DateTime? UpdatedTimestamp { get; set; }
        public DateTime? CreatedTimestamp { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public LocalizationRecordsGrouping? Group { get; set; }
    }
}
