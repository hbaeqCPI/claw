using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class DOCXRecordSource : DOCXRecordSourceDetail
    {
        public DOCXMain? DOCXMain { get; set; }

        public DOCXDataSource? DOCXDataSource { get; set; }

        public List<DOCXRecordSourceFilter>?  DOCXRecordSourceFilters { get; set; }
        public List<DOCXRecordSourceFilterUser>?  DOCXRecordSourceFiltersUser { get; set; }
    }

    public class DOCXRecordSourceDetail : BaseEntity
    {
        [Key]
        public int RecSourceId { get; set; }

        [Required]
        public int DOCXId { get; set; }

        [Required]
        public int DataSourceId { get; set; }

        [Required]
        public int ParentRecSourceId { get; set; }

        public int EntryOrder { get; set; }

    }
}
