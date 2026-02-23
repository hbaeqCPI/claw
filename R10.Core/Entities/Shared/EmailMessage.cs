using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Entities
{
    public class EmailMessage
    {
        public EmailMessage(string subject, string body)
        {
            Subject = subject;
            Body = body;
        }

        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
