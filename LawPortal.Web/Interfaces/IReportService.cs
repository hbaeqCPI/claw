using Microsoft.AspNetCore.Mvc;
using LawPortal.Web.Areas;
using System.Threading.Tasks;

namespace LawPortal.Web.Interfaces
{
    public interface IReportService
    {
        Task<IActionResult> GetReport(object obj, ReportType rt);
    }
}
