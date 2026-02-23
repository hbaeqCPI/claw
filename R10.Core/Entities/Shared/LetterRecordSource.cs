using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class LetterRecordSource : LetterRecordSourceDetail
    {
        public LetterMain? LetterMain { get; set; }

        public LetterDataSource? LetterDataSource { get; set; }

        public List<LetterRecordSourceFilter>?  LetterRecordSourceFilters { get; set; }
        public List<LetterRecordSourceFilterUser>?  LetterRecordSourceFiltersUser { get; set; }
    }

    public class LetterRecordSourceDetail : BaseEntity
    {
        [Key]
        public int RecSourceId { get; set; }

        [Required]
        public int LetId { get; set; }

        [Required]
        public int DataSourceId { get; set; }

        [Required]
        public int ParentRecSourceId { get; set; }

        public int EntryOrder { get; set; }

    }
}
