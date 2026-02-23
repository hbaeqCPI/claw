using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public class DataQueryTag : BaseEntity
    {
        [Key]
        public int DQTagId { get; set; }

        public int QueryId { get; set; }
        [Display(Name = "Tag")]
        public string? Tag { get; set; }

        public DataQueryMain? DataQuery { get; set; }
    }
}
