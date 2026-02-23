
namespace R10.Web.Areas.Shared.ViewModels
{
    public class CustomReportUploadViewModel : CustomReportViewModel
    {
        public bool Updating { get; set; }

        public bool IsOwner { get; set; }
        public IEnumerable<IFormFile>? Files { get; set; }
    }
}
