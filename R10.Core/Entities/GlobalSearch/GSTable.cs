using System.ComponentModel.DataAnnotations;


namespace R10.Core.Entities.GlobalSearch
{
    public class GSTable
    {
        [Key]
        public int TableId { get; set; }

        public bool IsDocContent { get; set; }

        public string? TableAlias { get; set; }

        // fields not relevant are not defined here but in the SQL db
        public List<GSField> GSFields { get; set; }
    }
}
