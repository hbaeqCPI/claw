using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class QELayout: BaseEntity
    {
        [Key]
        public int LayoutID { get; set; }

        [Required]
        public int QESetupID { get; set; }

        [Required]
        public int DataSourceID { get; set; }
        
        public string?  Subject { get; set; }

        public string?  Body { get; set; }

        public string?  Header { get; set; }

        public string?  Footer { get; set; }

    }
}
