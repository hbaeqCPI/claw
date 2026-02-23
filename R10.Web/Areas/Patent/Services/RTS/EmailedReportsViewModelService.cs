using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using R10.Web.Areas.Patent.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace R10.Web.Areas.Patent.Services
{
    public class EmailedReportsViewModelService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IHostingEnvironment _env;
        private IMemoryCache _cache;

        private static string CacheKeyLogs;
        private List<RTSEmailedReportViewModel> emailedReports;

        public EmailedReportsViewModelService(
                                  IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
                                  IHostingEnvironment env, IMemoryCache memoryCache)
        {
            CacheKeyLogs = "RTSEmailedReportCacheKey";
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
            _cache = memoryCache;
        }

        public List<RTSReportLogDateViewModel> GetSentDates(string clientCode, string reportType)
        {
            List<RTSReportLogDateViewModel> resultList = new List<RTSReportLogDateViewModel>();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("UpdateService")))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "Select * From vwReportLogDates Where AnnuityCode=@AnnuityCode And ReportType=@ReportType Order By SentDate DESC";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = sqlConnection;

                    SqlParameter paramClientCode = new SqlParameter();
                    paramClientCode.ParameterName = "@AnnuityCode";
                    paramClientCode.Value = clientCode;
                    cmd.Parameters.Add(paramClientCode);

                    SqlParameter paramReportType = new SqlParameter();
                    paramReportType.ParameterName = "@ReportType";
                    paramReportType.Value = reportType;
                    cmd.Parameters.Add(paramReportType);

                    sqlConnection.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        RTSReportLogDateViewModel current = new RTSReportLogDateViewModel();
                        current.AnnuityCode = (string)reader["AnnuityCode"];
                        current.Sender = (string)reader["Sender"];
                        current.Recipient = (string)reader["Recipient"];
                        current.ReportType = (string)reader["ReportType"];
                        current.SentDate = (DateTime)reader["SentDate"];

                        resultList.Add(current);
                    }
                }
            }
            return resultList;
        }

        public List<RTSReportLogViewModel> GetReports(string clientCode, string reportType, DateTime reportDate)
        {
            List<RTSReportLogViewModel> resultList = new List<RTSReportLogViewModel>();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("UpdateService")))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "Select * From vwReportLog Where AnnuityCode=@AnnuityCode And ReportType2=@ReportType And SentDate2=@SentDate";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = sqlConnection;

                    SqlParameter paramClientCode = new SqlParameter();
                    paramClientCode.ParameterName = "@AnnuityCode";
                    paramClientCode.Value = clientCode;
                    cmd.Parameters.Add(paramClientCode);

                    SqlParameter paramReportType = new SqlParameter();
                    paramReportType.ParameterName = "@ReportType";
                    paramReportType.Value = reportType;
                    cmd.Parameters.Add(paramReportType);

                    SqlParameter paramReportDate = new SqlParameter();
                    paramReportDate.ParameterName = "@SentDate";
                    paramReportDate.Value = reportDate;
                    cmd.Parameters.Add(paramReportDate);

                    sqlConnection.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        RTSReportLogViewModel current = new RTSReportLogViewModel();
                        current.LogId = (int)reader["LogId"];
                        current.ClientCode = (string)reader["ClientCode"];
                        current.AnnuityCode = (string)reader["AnnuityCode"];
                        current.Sender = (string)reader["Sender"];
                        current.Recipient = (string)reader["Recipient"];
                        current.ReportType = (string)reader["ReportType"];
                        current.SentDate = (DateTime)reader["SentDate"];
                        current.Path = (string)reader["Path"];
                        current.BatchId = (string)((reader["BatchId"] == DBNull.Value) ? "" : reader["BatchId"]);
                        current.SentDate2 = (DateTime)reader["SentDate2"];
                        current.ReportType2 = (string)reader["ReportType2"];
                        current.ReportTypeDesc = (string)reader["ReportTypeDesc"];

                        resultList.Add(current);
                    }
                }
            }
            return resultList;
        }

        public List<RTSReportLogDateViewModel> GetSentDates(string clientCode)
        {
            List<RTSReportLogDateViewModel> resultList = new List<RTSReportLogDateViewModel>();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("UpdateService")))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "Select top 24 convert(smalldatetime, convert(varchar(10), SentDate, 101)) as SentDate, max(sender) as sender, max(recipient) as recipient From vwReportLogDates Where AnnuityCode=@AnnuityCode group by convert(smalldatetime, convert(varchar(10), SentDate, 101)) Order By 1 DESC";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = sqlConnection;

                    SqlParameter paramClientCode = new SqlParameter();
                    paramClientCode.ParameterName = "@AnnuityCode";
                    paramClientCode.Value = clientCode;
                    cmd.Parameters.Add(paramClientCode);

                    sqlConnection.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        RTSReportLogDateViewModel current = new RTSReportLogDateViewModel();
                        current.Sender = (string)reader["Sender"];
                        current.Recipient = (string)reader["Recipient"];
                        current.SentDate = (DateTime)reader["SentDate"];

                        resultList.Add(current);
                    }
                }
            }
            return resultList;
        }

        public string GetCompareReportsFolder()
        {
            string reportPath = _configuration.GetValue<string>("UpdateService:UpdateServiceReportPath");//GetSection("UpdateService").
            string pdfFolder;

            if (System.IO.Directory.Exists(reportPath))
                pdfFolder = reportPath;
            else
                pdfFolder = System.IO.Path.Combine(_env.WebRootPath,reportPath);
            if(!pdfFolder.EndsWith("\\"))
                pdfFolder += "\\";
            return pdfFolder;
        }

        public List<RTSEmailedReportViewModel> GetEmailedReports(string clientCode, string reportTypes)
        {
            List<RTSEmailedReportViewModel> emailedReports;
            if (_cache.TryGetValue(CacheKeyLogs, out emailedReports))
                return emailedReports;

            emailedReports = new List<RTSEmailedReportViewModel>();
            List<RTSReportLogDateViewModel> reportLogDates = GetSentDates(clientCode);
            
            foreach(RTSReportLogDateViewModel reportLogDate in reportLogDates)
            {
                RTSEmailedReportViewModel emailedReport = new RTSEmailedReportViewModel()
                {
                    AnnuityCode = reportLogDate.AnnuityCode,
                    Recipient = reportLogDate.Recipient,
                    Sender = reportLogDate.Sender,
                    ReportType = reportLogDate.ReportType,
                    SentDate = reportLogDate.SentDate,
                };

                emailedReports.Add(emailedReport);
            }

            List<RTSReportLogViewModel> reportLogs = new List<RTSReportLogViewModel>();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("UpdateService")))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "select convert(smalldatetime, convert(varchar(10), SentDate, 101)) as SentDate, reporttype, [path], batchid From vwReportLog Where ClientCode = @ClientCode And @ReportTypes like '%,'+ReportType+',%'";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = sqlConnection;

                    SqlParameter paramClientCode = new SqlParameter();
                    paramClientCode.ParameterName = "@ClientCode";
                    paramClientCode.Value = clientCode;
                    cmd.Parameters.Add(paramClientCode);

                    SqlParameter paramReportTypes = new SqlParameter();
                    paramReportTypes.ParameterName = "@ReportTypes";
                    paramReportTypes.Value = "("+reportTypes+")";
                    cmd.Parameters.Add(paramReportTypes);

                    sqlConnection.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        RTSReportLogViewModel current = new RTSReportLogViewModel();
                        current.ReportType = (string)reader["ReportType"];
                        current.SentDate = (DateTime)reader["SentDate"];
                        current.Path = (string)reader["Path"];
                        current.BatchId = (string)((reader["BatchId"] == DBNull.Value) ? "" : reader["BatchId"]);

                        reportLogs.Add(current);
                    }
                }
            }

            string pdfFolder = string.Empty;

            foreach(RTSReportLogViewModel reportLog in reportLogs)
            {
                RTSEmailedReportViewModel emailedReport = emailedReports.FirstOrDefault(c=>c.SentDate== reportLog.SentDate);
                if (emailedReport != null)
                {
                    if(!(reportLog.Path==""|| reportLog.Path == null))
                    {
                        string path = reportLog.Path.Substring(8);
                        switch (reportLog.ReportType)
                        {
                            case "U":
                                emailedReport.Biblio = pdfFolder + path.Replace('/', '\\');
                                break;
                            case "V":
                                emailedReport.Assignment = pdfFolder + path.Replace('/', '\\');
                                break;
                            case "C":
                                emailedReport.Compare = pdfFolder + path.Replace('/', '\\');
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(1)).SetSize(1);

            _cache.Set(CacheKeyLogs, emailedReports, cacheEntryOptions);

            return emailedReports;
        }

        public List<RTSReportLogDateViewModel> GetReportInfo(string clientCode, string reportType, DateTime reportDate)
        {
            List<RTSReportLogDateViewModel> resultList = new List<RTSReportLogDateViewModel>();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("UpdateService")))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "Select * From vwReportLogDates Where AnnuityCode=@AnnuityCode And ReportType=@ReportType And SentDate=@SentDate Order By SentDate DESC";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = sqlConnection;

                    SqlParameter paramClientCode = new SqlParameter();
                    paramClientCode.ParameterName = "@AnnuityCode";
                    paramClientCode.Value = clientCode;
                    cmd.Parameters.Add(paramClientCode);

                    SqlParameter paramReportType = new SqlParameter();
                    paramReportType.ParameterName = "@ReportType";
                    paramReportType.Value = reportType;
                    cmd.Parameters.Add(paramReportType);

                    SqlParameter paramReportDate = new SqlParameter();
                    paramReportDate.ParameterName = "@SentDate";
                    paramReportDate.Value = reportDate;
                    cmd.Parameters.Add(paramReportDate);

                    sqlConnection.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        RTSReportLogDateViewModel current = new RTSReportLogDateViewModel();
                        current.AnnuityCode = (string)reader["AnnuityCode"];
                        current.Sender = (string)reader["Sender"];
                        current.Recipient = (string)reader["Recipient"];
                        current.ReportType = (string)reader["ReportType"];
                        current.SentDate = (DateTime)reader["SentDate"];

                        resultList.Add(current);
                    }
                }
            }
            return resultList;
        }

        public List<RTSReportLogDateViewModel> GetReportInfo(string clientCode)
        {
            List<RTSReportLogDateViewModel> resultList = new List<RTSReportLogDateViewModel>();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("UpdateService")))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "Select * From vwReportLogDates Where AnnuityCode=@AnnuityCode Order By SentDate DESC";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = sqlConnection;

                    SqlParameter paramClientCode = new SqlParameter();
                    paramClientCode.ParameterName = "@AnnuityCode";
                    paramClientCode.Value = clientCode;
                    cmd.Parameters.Add(paramClientCode);

                    sqlConnection.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        RTSReportLogDateViewModel current = new RTSReportLogDateViewModel();
                        current.AnnuityCode = (string)reader["AnnuityCode"];
                        current.Sender = (string)reader["Sender"];
                        current.Recipient = (string)reader["Recipient"];
                        current.ReportType = (string)reader["ReportType"];
                        current.SentDate = (DateTime)reader["SentDate"];

                        resultList.Add(current);
                    }
                }
            }
            return resultList;
        }
    }
}
