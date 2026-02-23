using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class CountryApplicationCopySetting 
    {
        [Key]
        public int CopySettingId { get; set; }
        
        public string? FieldDesc { get; set; }
        public string? FieldName { get; set; }

        public bool Copy { get; set; }
        public string? UserName { get; set; }
    }

    public class CountryApplicationCopySettingChild
    {
        public string? FromField { get; set; }
        public string? ToField { get; set; }
    }
    
}
