using R10.Core.Identity;

namespace R10.Web.Api.Models
{
    public class SystemStatusParam
    {
        public SystemStatus? SystemStatus { get; set; }
        public bool UpdateSecurityStamp { get; set; }
    }
}
