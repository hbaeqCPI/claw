using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.DTOs
{
    public class FormExtractDTO
    {
        public string? FieldName { get; set; }
        public string? FieldData { get; set; }
        public double Confidence { get; set; }
    }

}
