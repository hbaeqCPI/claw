using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.PatClearance
{
    public class PacDiscussion : PacDiscussionDetail
    {
        public List<PacDiscussionReply>? Replies { get; set; }

        public PacClearance? Clearance { get; set; }
    }

    public class PacDiscussionDetail : BaseEntity
    {
        [Key]
        public int DiscussId { get; set; }

        [Required]
        public int PacId { get; set; }

        [UIHint("TextArea")]
        public string DiscussionMsg { get; set; }

        public int OrderOfEntry { get; set; }

        [StringLength(450)]
        [Required]
        public string UserId { get; set; }

        public bool IsPrivate { get; set; }
    }
}
