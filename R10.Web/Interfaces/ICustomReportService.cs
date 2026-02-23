using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface ICustomReportService
    {
        Task<string> GetSQLExpr(int id, string? userEmail = null);
        Task<DataTable> RunQuery(string userId, int queryId);
        //List<object> ConvertDataTable(DataTable dt);

        Task<MemoryStream> GetCustomReport(int id);
        Task<MemoryStream> GetCustomReport(string queryName);
        Task<MemoryStream> ConvertServerRDLToLocalRDL(byte[] reportDefinition);

        string PrepareFileName(string name);
        Task AddDataQuery(DataQueryMain query);
        Task<DataQueryMain> GetDataQuery(string queryName);
        Task<string> ConvertLocalRDLToServerRDL(string rdlContents);
        int GetQueryId(string rdlContents);
        Task<string> GetDataPermissionTextByReportId(int reportId);
    }
}
