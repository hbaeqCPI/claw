using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IReportService
    {
        Task<IActionResult> GetReport(Object obj, ReportType rt);
        Task<EmailReportViewModel> SaveEmailReport(Object obj, ReportType rt);
        IActionResult GetGeneratedReport(string generatedReportName);
        Task<EmailSenderResult> EmailReport(EmailReportViewModel emailData);
        string GetOutputFormatExtension(int reportFormat);
        string GetErrorMessage();
        string GetUnhandledErrorMessage();
    }
}
