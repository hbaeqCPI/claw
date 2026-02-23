using DocumentFormat.OpenXml.Drawing.Charts;
using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CustomReportDetailViewModel : CustomReport
    {
        public bool IsMyReport { get; set; } = true;
        [Required(ErrorMessage = "Report File is Required.")]
        public IEnumerable<IFormFile>? Files { get; set; }
        public int ReportFormat { get; set; }
        public int ReportFormatForCustomReport { get; set; }
        [Display(Name = "Report Data Source")]
        public string? ReportDataSource { get; set; }
    }
}
