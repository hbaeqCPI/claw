using System;

namespace R10.Web.Models.EmailAddInModels
{
    // the fields below should correspond to the fields in the email attachment object of the Outlook API
    public class OutlookAttachment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public int Size { get; set; }
        public bool IsInline { get; set; }
        public string ContentId { get; set; }
        public Byte[] ContentBytes { get; set; }
    }

   
}
