using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DisclosureCopyClearanceSetting 
    {
        [Key]
        public int CopySettingId { get; set; }
        
        public string? FieldDesc { get; set; }
        public string? FieldName { get; set; }

        public string? DestFieldDesc { get; set; }
        public string? DestFieldName { get; set; }

        public bool Copy { get; set; }
    }
    
}
