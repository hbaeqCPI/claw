using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    public class DocsOutCriteriaDTO
    {
        public string? SystemType { get; set; }
        public string? DocumentCode { get; set; }
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }
    }
}