using Microsoft.Extensions.Options;
using R10.Web.Helpers;
using RS2005D;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using R10.Web.Interfaces;
using Microsoft.AspNetCore.Http;
using R10.Core.Interfaces;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.AMS;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Shared;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace R10.Web.Services.EmailAddIn
{
    public class ReportDeployService : IReportDeployService
    {
        private readonly ReportSettings _reportSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ReportDeployService(IOptions<ReportSettings> reportSettings, IHostingEnvironment hostingEnvironment)
        {
            _reportSettings = reportSettings.Value;
            _hostingEnvironment = hostingEnvironment;
        }

        private ReportingService2005SoapClient GetServiceClient(string reportDeployServiceUrl)
        {
            HttpBindingBase binding;
            if (_reportSettings.ReportServiceUrl.StartsWith("https:"))
            {
                binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
                if (_reportSettings.UseNtlmAuthentication)
                    ((BasicHttpsBinding)binding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                else
                    ((BasicHttpsBinding)binding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }
            else
            {
                binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
                if (_reportSettings.UseNtlmAuthentication)
                    ((BasicHttpBinding)binding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                else
                    ((BasicHttpBinding)binding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }

            ReportingService2005SoapClient serviceClient = new ReportingService2005SoapClient(binding, new EndpointAddress(reportDeployServiceUrl));
            binding.MaxReceivedMessageSize = _reportSettings.MaxFileSizeInMB * 1000000;

            if (_reportSettings.UseNtlmAuthentication)
            {
                //Setup access credentials.Here use windows credentials.
                var clientCredentials = new NetworkCredential(_reportSettings.UserName, _reportSettings.Password, _reportSettings.Domain);

                serviceClient.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
                serviceClient.ClientCredentials.Windows.ClientCredential = clientCredentials;
            }

            return serviceClient;
        }

        private async Task<bool> DeployReport(string reportDeployServiceUrl, string filePath, string reportName, string parentPath)
        {
            ReportingService2005SoapClient serviceClient = GetServiceClient(reportDeployServiceUrl);

            byte[] file = File.ReadAllBytes(filePath);
            try
            {
                await serviceClient.CreateReportAsync(null, reportName, parentPath, true, file, null);
                DataSource[] dataSources = (await serviceClient.GetItemDataSourcesAsync(parentPath+ "/" +reportName)).DataSources;
                foreach(DataSource datasource in dataSources)
                {
                    ((DataSourceDefinition)datasource.Item).CredentialRetrieval = CredentialRetrievalEnum.None;
                    await serviceClient.SetItemDataSourcesAsync(null, parentPath + "/" + reportName, dataSources);
                }
                
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        private async Task<bool> DeployDataSource(string reportDeployServiceUrl, string filePath, string reportName, string parentPath)
        {
            ReportingService2005SoapClient serviceClient = GetServiceClient(reportDeployServiceUrl);

            var df = await serviceClient.GetDataSourceContentsAsync(filePath);
            try
            {
                
                await serviceClient.CreateDataSourceAsync(null, reportName, parentPath, true, df.Definition, null);
                //DataSource[] dataSources = (await serviceClient.GetItemDataSourcesAsync(parentPath + "/" + reportName)).DataSources;
                //foreach (DataSource datasource in dataSources)
                //{
                //    ((DataSourceDefinition)datasource.Item).CredentialRetrieval = CredentialRetrievalEnum.None;
                //    await serviceClient.SetItemDataSourcesAsync(null, parentPath + "/" + reportName, dataSources);
                //}

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        private string[] GetAllReportDeployServiceUrls()
        {
            return _reportSettings.ReportDeployServiceUrls.Split("|");
        }

        private FileInfo[] GetAllReportFileInfos(string directory, string extension)
        {

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            DirectoryInfo d = new DirectoryInfo(directory);

            FileInfo[] Files = d.GetFiles("*" + extension);

            return Files;
        }

        public string GetCPIReportsDirectory()
        {
            var CPIReportsDirectory = Path.Combine(_hostingEnvironment.ContentRootPath, "UserFiles/CPIReports");
            if (!Directory.Exists(CPIReportsDirectory))
                Directory.CreateDirectory(CPIReportsDirectory);
            return CPIReportsDirectory;
        }

        public async System.Threading.Tasks.Task<bool> DeployCPIPredefinedReports()
        {
            bool success = true;
            if(await DeployCPIDateSource())
            {
                if(await DeployCPIReports())
                {

                }
                else
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }
            return success;
        }

        public async System.Threading.Tasks.Task<bool> DeployCPIDateSource()
        {
            bool success = true;
            string extension = ".rds";
            FileInfo[] files = GetAllReportFileInfos(GetCPIReportsDirectory(), extension);
            if (files.Length != 0)
            {
                string[] reportDeployServiceUrls = GetAllReportDeployServiceUrls();
                string parentFolder = _reportSettings.ClientFolder.TrimEnd('/');
                parentFolder = parentFolder.Replace("/Reports", "/Data Sources");
                foreach (FileInfo fileInfo in files)
                {
                    bool successOnSingleFile = true;
                    foreach (string url in reportDeployServiceUrls)
                    {
                        if (!await DeployDataSource(url, fileInfo.FullName, fileInfo.Name.Substring(0, fileInfo.Name.Length - 4), parentFolder))
                        {
                            successOnSingleFile = false;
                            success = false;
                        }
                    }
                    if (successOnSingleFile)
                    {
                        File.Delete(fileInfo.FullName);
                    }
                }
            }
            return success;
        }

        public async System.Threading.Tasks.Task<bool> DeployCPIReports()
        {
            bool success = true;
            string extension = ".rdl";
            FileInfo[] files = GetAllReportFileInfos(GetCPIReportsDirectory(), extension);
            if (files.Length != 0)
            {
                string[] reportDeployServiceUrls = GetAllReportDeployServiceUrls();
                string parentFolder = _reportSettings.ClientFolder.TrimEnd('/');
                foreach (FileInfo fileInfo in files)
                {
                    bool successOnSingleFile = true;
                    foreach (string url in reportDeployServiceUrls)
                    {

                        if (!await DeployReport(url, fileInfo.FullName, fileInfo.Name.Substring(0, fileInfo.Name.Length - 4), parentFolder))
                        {
                            successOnSingleFile = false;
                            success = false;
                        }
                    }
                    if (successOnSingleFile)
                    {
                        File.Delete(fileInfo.FullName);
                    }
                }
            }
            return success;
        }

        public string GetCustomReportsDirectory()
        {
            var customReportsDirectory = Path.Combine(_hostingEnvironment.ContentRootPath, "UserFiles/CustomReports");
            if (!Directory.Exists(customReportsDirectory))
                Directory.CreateDirectory(customReportsDirectory);
            return customReportsDirectory;
        }

        public async System.Threading.Tasks.Task<bool> DeployCustomReports()
        {
            bool success = true;
            string extension = ".rdl";
            FileInfo[] files = GetAllReportFileInfos(GetCustomReportsDirectory(), extension);
            if (files.Length != 0)
            {
                string[] reportDeployServiceUrls = GetAllReportDeployServiceUrls();
                string parentFolder = _reportSettings.ClientFolder.TrimEnd('/');
                foreach (FileInfo fileInfo in files)
                {
                    bool successOnSingleFile = true;
                    foreach (string url in reportDeployServiceUrls)
                    {
                    
                        if (!await DeployReport(url, fileInfo.FullName, "CR_" + fileInfo.Name.Substring(0, fileInfo.Name.Length - 4), parentFolder))
                        {
                            successOnSingleFile = false;
                            success = false;
                        }
                    }
                    if (successOnSingleFile)
                    {
                        File.Delete(fileInfo.FullName);
                    }
                }
            }
            return success;
        }

        public async System.Threading.Tasks.Task<bool> DeployCustomReport(string filePath, string reportName)
        {
            bool success = true;
            if (File.Exists(filePath))
            {
                string[] reportDeployServiceUrls = GetAllReportDeployServiceUrls();
                string parentFolder = _reportSettings.ClientFolder.TrimEnd('/');

                await DeleteReport(reportName);

                foreach (string url in reportDeployServiceUrls)
                {
                    if (!await DeployReport(url, filePath, "CR_" + reportName, parentFolder))
                    {
                        success = false;
                    }
                }
            }
            else
            {
                success = false;
            }
            if (success)
            {
                File.Delete(filePath);
            }
            return success;
        }

        public async System.Threading.Tasks.Task<IQueryable<CatalogItem>> ListReports()
        {
            //suppose all the report servers are having same reports

            ReportingService2005SoapClient serviceClient = GetServiceClient(GetAllReportDeployServiceUrls()[0]);
            string parentFolder = _reportSettings.ClientFolder.TrimEnd('/');
            CatalogItem[] reports = (await serviceClient.ListChildrenAsync(parentFolder, false)).CatalogItems;
            return reports.AsQueryable();
        }

        public async System.Threading.Tasks.Task<IQueryable<CatalogItem>> ListReports(string reportDeployServiceUrl)
        {
            //suppose all the report servers are having same reports

            ReportingService2005SoapClient serviceClient = GetServiceClient(reportDeployServiceUrl);
            string parentFolder = _reportSettings.ClientFolder.TrimEnd('/');
            CatalogItem[] reports = (await serviceClient.ListChildrenAsync(parentFolder, false)).CatalogItems;
            return reports.AsQueryable();
        }

        public async System.Threading.Tasks.Task<bool> DeleteReport(string reportName)
        {
            var depolyServices = GetAllReportDeployServiceUrls();
            foreach(string deployService in depolyServices)
            {
                ReportingService2005SoapClient serviceClient = GetServiceClient(deployService);
                string parentFolder = _reportSettings.ClientFolder;
                try
                {
                    if ((await ListReports(deployService)).Any(c=> c.Name.Equals("CR_" + reportName)))
                    {
                        await serviceClient.DeleteItemAsync(null, parentFolder + "CR_" + reportName);
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return true;
        }

        public async System.Threading.Tasks.Task<IQueryable> GetCustomReportNames()
        {
            return (await ListReports()).Where(c => c.Name.StartsWith("CR_")).Select(c => new { ReportName = c.Name.Substring(3) }).OrderBy(c=>c.ReportName);
        }

        public async System.Threading.Tasks.Task<byte[]> GetReportDefinition(string reportName)
        {
            ReportingService2005SoapClient serviceClient = GetServiceClient(GetAllReportDeployServiceUrls()[0]);
            var report = (await serviceClient.GetReportDefinitionAsync(_reportSettings.ClientFolder + "CR_" + reportName)).Definition;
            return report;
        }

        public async System.Threading.Tasks.Task<int> GetCustomQueryId(string reportName)
        {
            var report = await GetReportDefinition(reportName);
            var text = System.Text.Encoding.UTF8.GetString(report);

            var searchPosition = "<ReportParameter Name=\"p3\">";
            var postfix = text.Substring(text.IndexOf(searchPosition) + searchPosition.Length);
            var valuePosition = "<Value>";
            var endValuePostion = "</Value>";
            var Id = postfix.Substring(postfix.IndexOf(valuePosition) + valuePosition.Length, postfix.IndexOf(endValuePostion)- postfix.IndexOf(valuePosition) - valuePosition.Length);
            return int.Parse(Id);
        }
    }
}
