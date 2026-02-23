using R10.Core.Entities.Documents;
using System.ComponentModel.DataAnnotations;


namespace R10.Core.Entities.GlobalSearch
{
    public class GSField
    {
        [Key]
        public int FieldId { get; set; }

        public int TableId { get; set; }
        public int ScreenId { get; set; }
        public string FieldName { get; set; }
        public string FieldLabelLong { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsAutoSearch { get; set; }
        public int EntryOrder { get; set; }
        public string? SQLWhere { get; set; }
        public bool IsDefaultCriteria { get; set; }

        // fields not relevant are not defined here but in the SQL db

        public GSScreen GSScreen { get; set; }
        public GSTable GSTable { get; set; }

        public List<DocVerificationSearchField> DocVerificationSearchFields { get; set; }
    }
}
