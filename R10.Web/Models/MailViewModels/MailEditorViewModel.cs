using Microsoft.Graph;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Models.MailViewModels
{
    public class MailEditorViewModel
    {
        public string? MailboxName { get; set; }
        public string? Id { get; set; }

        [Required]
        [Display(Name = "To")]
        public string ToRecipients { get; set; }

        [Display(Name = "Cc")]
        public string? CcRecipients { get; set; }

        [Display(Name = "Bcc")]
        public string? BccRecipients { get; set; }

        [Display(Name = "Subject")]
        public string? Subject { get; set; }

        [Required]
        [Display(Name = "Body")]
        public string Body { get; set; }

        public Message? OriginalMessage { get; set; }

        public string? SendURL { get; set; }
    }

    public static class MailEditor
    {
        public const string Reply = "reply";
        public const string ReplyAll = "replyAll";
        public const string Forward = "forward";
    }
}
