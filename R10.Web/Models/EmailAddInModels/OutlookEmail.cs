using System;
using System.Collections.Generic;

namespace R10.Core.DTOs
{

    // the fields below should correspond to the fields in the email object of the Outlook API
    public class OutlookEmail
    {
        public string Id { get; set; }

        public Dictionary<string, EmailAddress> Sender { get; set; }
        public Dictionary<string, EmailAddress> From { get; set; }
        public Dictionary<string, EmailAddress>[] ToRecipients { get; set; }
        public Dictionary<string, EmailAddress>[] CcRecipients { get; set; }
        public Dictionary<string, EmailAddress>[] BccRecipients { get; set; }
        public Dictionary<string, EmailAddress>[] ReplyTo { get; set; }

        public string Subject { get; set; }
        public EmailBody Body { get; set; }
        public string BodyPreview { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public DateTime ReceivedDateTime { get; set; }
        public DateTime SentDateTime { get; set; }
        public bool HasAttachments { get; set; }
        public string Importance { get; set; }
        //public bool IsDeliveryReceiptRequested { get; set; }
        public bool IsReadReceiptRequested { get; set; }
        public bool IsRead { get; set; }
        public bool IsDraft { get; set; }

       
    }

    public class EmailBody
    {
        public string ContentType { get; set; }
        public string Content { get; set; }
    }

    public class EmailAddress
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
