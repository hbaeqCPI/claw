using Microsoft.AspNetCore.Mvc;
using LawPortal.Web.Areas;
using LawPortal.Web.Interfaces;
using System.Threading.Tasks;

namespace LawPortal.Web.Services
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
