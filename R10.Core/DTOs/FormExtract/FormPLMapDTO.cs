using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class FormPLMapDTO
    {
        public int MapSourceId { get; set; }
        public string? DocumentDescription { get; set; }
        public string? FormType { get; set; }
        public string? FormName { get; set; }
        
        [NotMapped]
        public string? FormExtractLink { get; set; }


    }
}
