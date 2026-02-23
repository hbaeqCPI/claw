using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSDiscussion : DMSDiscussionDetail
    {
        public List<DMSDiscussionReply>? Replies { get; set; }

        public Disclosure? Disclosure { get; set; }
    }

    public class DMSDiscussionDetail : BaseEntity
    {
        [Key]
        public int DiscussId { get; set; }

        [Required]
        public int DMSId { get; set; }

        [UIHint("TextArea")]
        public string? DiscussionMsg { get; set; }

        public int OrderOfEntry { get; set; }

        public string? Recommendation { get; set; }

        [StringLength(450)]
        [Required]
        public string? UserId { get; set; }

        public bool IsPrivate { get; set; }
        public bool IsPreview { get; set; }
        public bool IsPreviewPrivate { get; set; }
    }
}
