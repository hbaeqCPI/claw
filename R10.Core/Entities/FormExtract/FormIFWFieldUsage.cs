using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.FormExtract
{
    public class FormIFWFieldUsage
    {
        [Key]
        public int UsageId { get; set; }

        public int DocTypeId { get; set; }
        public string? FieldName { get; set; }
        public string? FieldUsage { get; set; }
        public string? FieldLabel { get; set; }
        
        public int EntryOrder { get; set; }
        public List<FormIFWDataExtract>? FormIFWDataExtracts { get; set; }
    }
}
