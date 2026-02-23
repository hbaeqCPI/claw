using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEPatSearchView
    {
        public int SearchId { get; set; }
        public string? CriteriaName { get; set; }
        public DateTime SearchDate { get; set; }
        //public string? NewEntries { get; set; }
    }
}
