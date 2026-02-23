using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Clearance
{
    public class TmcDiscussion : TmcDiscussionDetail
    {
        public List<TmcDiscussionReply>? Replies { get; set; }

        public TmcClearance? Clearance { get; set; }
    }

    public class TmcDiscussionDetail : BaseEntity
    {
        [Key]
        public int DiscussId { get; set; }

        [Required]
        public int TmcId { get; set; }

        [UIHint("TextArea")]
        public string? DiscussionMsg { get; set; }

        public int OrderOfEntry { get; set; }

        [StringLength(450)]
        [Required]
        public string UserId { get; set; }

        public bool IsPrivate { get; set; }
    }
}
