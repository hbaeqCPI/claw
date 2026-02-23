using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSQuestionGroup : BaseEntity
    {
        [Key]
        public int GroupId { get; set; }

        [Display(Name = "Group Name")]
        public string? GroupName { get; set; }

        [Display(Name = "Tab Order")]
        public int OrderOfEntry { get; set; }

        public string? ReviewerEntityFilter { get; set; }

        [NotMapped]
        public int[]? ReviewerEntityFilterList { get; set; }
        
        [NotMapped]
        public string? ReviewerEntityFilterStr { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }

        public List<DMSQuestionGuide>? DMSQuestionGuides { get; set; }
    }
}
