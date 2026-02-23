using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.GlobalSearch
{
    public class GSScreen
    {
        [Key]
        public int ScreenId { get; set; }

        public string SystemType { get; set; }
        public string ScreenCode { get; set; }
        public string ScreenName { get; set; }
        public bool IsEnabled { get; set; }
        public int EntryOrder { get; set; }

        public GSSystem GSSystem { get; set; }
        public List<GSField> GSFields{ get; set; }

        // fields not relevant are not defined here but in the SQL db
    }
}
