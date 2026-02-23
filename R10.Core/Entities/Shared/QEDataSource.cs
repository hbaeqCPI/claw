using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class QEDataSource : BaseEntity
    {
        [Key]
        public int DataSourceID { get; set; }

        [Required]
        [Display(Name = "System Type")]
        [StringLength(1)]
        public string?  SystemType { get; set; }

        [Required]
        [Display(Name = "Data Source Name")]
        [StringLength(30)]
        public string?  DataSourceName { get; set; }
        
        [Display(Name = "View Name")]
        [StringLength(60)]
        public string?  ViewName { get; set; }

        [Display(Name = "Image Table Name")]
        [StringLength(60)]
        public string?  ImageTableName { get; set; }


        [Display(Name = "Data Key")]
        [StringLength(30)]
        public string?  DataKey { get; set; }

        [Display(Name = "Order By")]
        [StringLength(50)]
        public string?  OrderBy { get; set; }

        [Display(Name = "In Use")]
        public bool InUse { get; set; }
        
        public List<QEMain>? QEsMain { get; set; }
        public List<QECustomField>? QECustomFields { get; set; }
    }

    public class QEDataSourceScreen {
        [Key]
        public int DataSourceScreenId { get; set; }
        public int DataSourceId { get; set; }
        public string? ScreenCode { get; set; }
    }
}
