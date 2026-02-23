using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class ReportSettings
    {
        public string ReportServiceUrl { get; set; }
        public string ReportDeployServiceUrls { get; set; }
        public bool UseNtlmAuthentication { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string ClientFolder { get; set; }
        public string ClientApiUrl { get; set; }
        public int MaxFileSizeInMB { get; set; }
        public string ReportServerDomain { get; set; }

        private readonly int DefaultTimeoutMinutes = 3;
        public int? CloseTimeoutInMinutes { get; set; }
        public int? OpenTimeoutInMinutes { get; set; }
        public int? ReceiveTimeoutInMinutes { get; set; }
        public int? SendTimeoutInMinutes { get; set; }

        public TimeSpan CloseTimeout => TimeSpan.FromMinutes(CloseTimeoutInMinutes > 0 ? CloseTimeoutInMinutes.Value : DefaultTimeoutMinutes);
        public TimeSpan OpenTimeout => TimeSpan.FromMinutes(OpenTimeoutInMinutes > 0 ? OpenTimeoutInMinutes.Value : DefaultTimeoutMinutes);
        public TimeSpan ReceiveTimeout => TimeSpan.FromMinutes(ReceiveTimeoutInMinutes > 0 ? ReceiveTimeoutInMinutes.Value : DefaultTimeoutMinutes);
        public TimeSpan SendTimeout => TimeSpan.FromMinutes(SendTimeoutInMinutes > 0 ? SendTimeoutInMinutes.Value : DefaultTimeoutMinutes);
    }
}
