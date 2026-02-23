using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocuSignSettings
    {
        public string ClientId { get; set; }
        public string AuthServer { get; set; }
        public string DeveloperServer { get; set; }
        public string ImpersonatedUserId { get; set; }
        public string PrivateKeyFile { get; set; }
    }


}
