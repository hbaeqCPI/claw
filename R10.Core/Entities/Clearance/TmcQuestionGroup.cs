using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Clearance
{
    public class TmcQuestionGroup : BaseEntity
    {
        [Key]
        public int GroupId { get; set; }

        [Display(Name = "Group Name")]
        public string GroupName { get; set; }

        [Display(Name = "Tab Order")]
        public int OrderOfEntry { get; set; }

        public List<TmcQuestionGuide>? TmcQuestionGuides { get; set; }
    }
}