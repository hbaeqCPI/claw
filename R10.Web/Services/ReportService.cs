using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas;
using R10.Web.Interfaces;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class ReportService : IReportService
    {
        public Task<IActionResult> GetReport(object obj, ReportType rt)
        {
            IActionResult result = new ContentResult
            {
                Content = "Report service not available",
                ContentType = "text/plain",
                StatusCode = 501
            };
            return Task.FromResult(result);
        }
    }
}
