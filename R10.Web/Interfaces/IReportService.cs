using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IReportService
    {
        Task<IActionResult> GetReport(object obj, ReportType rt);
    }
}
