using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace R10.Web.Helpers
{
    // model used in creating .msg file from email components
    public class MsgEmailModel
    {
        public MailAddress Sender { get; set; }                                 // In Outlook, this is separate from 'From'
        public MailAddress From { get; set; }
        public List<MailAddress> To { get; set; }
        public List<MailAddress> ReplyTo { get; set; }
        public List<MailAddress> Cc { get; set; }
        public List<MailAddress> Bcc { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }
        public string BodyPreview { get; set; }                                 // In Outlook, this the first 255 characters of the body

        public DateTime SentDate { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public DateTime? LastModified { get; set; }

        public bool IsHtml { get; set; } = true;                                // In Outlook, this is message.Body.Content == 'HTML'
        public bool IsDraft { get; set; } = false;
        public bool IsSent { get; set; } = true;                                // for email icon
        public bool IsReadReceiptRequested { get; set; } = false;
        public string Importance { get; set; }                                  // In Outlook: high, normal, low

        public List<string> Attachments { get; set; } = new List<string>();     // list of physical file names (example: QE)

        public List<ByteAttachment> ByteAttachments { get; set; }               // streamed/byte attachments (example: Outlook add-in)
                = new List<ByteAttachment>();

    }

     public class ByteAttachment
    {
        public ByteAttachment(string name, bool isInline, string contentId, Byte[] contentBytes )
        {
            this.Name = name;
            this.IsInline = isInline;
            this.ContentId = contentId;
            this.ContentBytes = contentBytes;
        }

        //public string Id { get; set; }
        public string Name { get; set; }
        //public string ContentType { get; set; }
        //public int Size { get; set; }
        public bool IsInline { get; set; }
        public string ContentId { get; set; }
        public Byte[] ContentBytes { get; set; }
    }

}
