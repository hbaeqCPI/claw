using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class GlobalUpdateLog
    {
        [Key]
        public int LogId { get; set; }
        [Display(Name = "Update Field")]
        public string?  UpdateField { get; set; }
        [Display(Name = "Old Value")]
        public string?  DataFrom { get; set; }
        [Display(Name = "New Value")]
        public string?  DataTo { get; set; }
        [Display(Name = "Update Criteria")]
        public string?  Criteria { get; set; }
        [Display(Name = "Remarks")]
        public string?  Remarks { get; set; }
        [Display(Name = "Record Count")]
        public int? RecCount { get; set; }
        [Display(Name = "Update Date")]
        public DateTime? UpdatedOn { get; set; }
        [Display(Name = "Updated By")]
        public string?  UpdatedBy { get; set; }
    }
}
