using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Services
{
    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool SendOnBehalfOf { get; set; }
        public string Sender { get; set; }
        public bool AllowSpoofing { get; set; }
    }
}
