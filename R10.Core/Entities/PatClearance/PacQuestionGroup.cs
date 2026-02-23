using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.PatClearance
{
    public class PacQuestionGroup : BaseEntity
    {
        [Key]
        public int GroupId { get; set; }

        [Display(Name = "Group Name")]
        public string GroupName { get; set; }

        [Display(Name = "Tab Order")]
        public int OrderOfEntry { get; set; }

        public List<PacQuestionGuide>? PacQuestionGuides { get; set; }
    }
}