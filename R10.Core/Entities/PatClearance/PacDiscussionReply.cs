using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.PatClearance
{

    public class PacDiscussionReply : BaseEntity
    {
        [Key]
        public int ReplyId { get; set; }

        [Required]
        public int DiscussId { get; set; }

        [UIHint("TextArea")]
        public string ReplyMsg { get; set; }

        public int OrderOfEntry { get; set; }

        [StringLength(450)]
        [Required]
        public string UserId { get; set; }

        public PacDiscussion? Discussion { get; set; }
    }
}
