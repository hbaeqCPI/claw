using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class LocalizationRecordsGrouping
    {
        [Key]
        public long Id { get; set; }
                
        public int ParentId { get; set; }

        [StringLength(450)]
        [Required]
        public string ResourceKey { get; set; }

        [StringLength(255)]        
        public string Description { get; set; }

        [StringLength(50)]
        public string System { get; set; }

        [StringLength(1000)]
        public string Menu { get; set; }

        public List<LocalizationRecords> LocalizationRecords { get; set; }

    }
}
