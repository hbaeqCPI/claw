using RS2005D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IReportDeployService
    {
        System.Threading.Tasks.Task<bool> DeployCustomReports();
        System.Threading.Tasks.Task<bool> DeployCustomReport(string filePath, string reportName);
        //System.Threading.Tasks.Task<CatalogItem[]> ListReports();
        System.Threading.Tasks.Task<IQueryable> GetCustomReportNames();
        System.Threading.Tasks.Task<int> GetCustomQueryId(string ReportName);
        string GetCustomReportsDirectory();
        System.Threading.Tasks.Task<byte[]> GetReportDefinition(string reportName);
        System.Threading.Tasks.Task<bool> DeleteReport(string reportName);
        System.Threading.Tasks.Task<bool> DeployCPIPredefinedReports();
    }
}
