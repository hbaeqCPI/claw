using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.DTOs
{
    public class OutlookLinkedCases
    {
        public int FileId { get; set; }
        public List<OutlookProcessedCases> DataKeyValue { get; set; }
    }

    public class OutlookProcessedCases
    {
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }
        public int DocId { get; set; }
    }
}
