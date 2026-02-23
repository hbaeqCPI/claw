using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities 
{
    public class SysCustomFieldSetting : BaseEntity
    {
        [Key]
        public int SettingId { get; set; }

        [Display(Name = "Table Name")]
        public string?  TableName { get; set; }

        [Display(Name = "Column Name")]
        public string?  ColumnName { get; set; }

        [Display(Name = "Column Label")]
        public string?  ColumnLabel { get; set; }

        [Display(Name = "Data Type")]
        public string?  DataType { get; set; }

        [Display(Name = "Editor Type")]
        public string?  EditorType { get; set; }

        [Display(Name = "Visible?")]
        public bool? Visible { get; set; }

        [Display(Name = "Order")]
        public int OrderOfEntry { get; set; }

        [NotMapped]
        public string?  ColumnValue { get; set; }

        [Display(Name = "Country App Search?")]
        public bool CountryAppSearch { get; set; }
    }
}
