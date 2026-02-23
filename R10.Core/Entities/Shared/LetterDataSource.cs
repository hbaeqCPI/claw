using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class LetterDataSource : LetterDataSourceDetail
    {
        public List<LetterRecordSource>? LetterRecordSources { get; set; }
        public List<LetterCustomField>? LetterCustomFields { get; set; }

    }

    public class LetterDataSourceDetail : BaseEntity
    {
        [Key]
        public int DataSourceId { get; set; }

        [StringLength(50)]
        [Display (Name = "Data Source")]
        public string?  DataSourceDescMain { get; set; }

        [StringLength(50)]
        public string?  DataSourceDescDtl { get; set; }

        [StringLength(100)]
        public string?  DataSourceMain { get; set; }

        [StringLength(100)]
        public string?  DataSourceDtl { get; set; }

        [StringLength(50)]
        public string?  DataKey { get; set; }

        [StringLength(50)]
        public string?  SourceAlias { get; set; }

        [StringLength(50)]
        public string?  DefaultOrderBy{ get; set; }

        [StringLength(50)]
        public string?  EntityTrigger { get; set; }

        //[StringLength(100)]
        //public string?  FilterExprMain { get; set; }

        //[StringLength(100)]
        //public string?  FilterExprDtl { get; set; }

        //[StringLength(450)]
        //public string?  SystemId { get; set; }

        // TODO: remove later
        [StringLength(1)]
        public string?  SystemType { get; set; }
    }
}
