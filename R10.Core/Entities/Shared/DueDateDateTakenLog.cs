using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{

    public class DueDateDateTakenLog 
    {
        [Key]
        public int LogId { get; set; }

        public int DDId { get; set; }

        [Display(Name = "Date Taken")]
        public DateTime? DateTaken { get; set; }

        [Display(Name = "Entered By")]
        public string? EnteredBy { get; set; }

        public DateTime? DateEntered { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[]? tStamp { get; set; }
    }


}
