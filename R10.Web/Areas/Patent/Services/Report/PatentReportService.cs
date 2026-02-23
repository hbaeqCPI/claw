using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Patent.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.Services
{
    public class PatentReportService
    {
        private readonly IApplicationDbContext _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PatentReportService(
                IApplicationDbContext repository
            , IHttpContextAccessor httpContextAccessor
            )
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public IDSCrossCheckData GetIDSCrossCheckData(PatIDSCrossCheckViewModel criteria)
        {
            IDSCrossCheckData data = new IDSCrossCheckData();
            data.FullDataViewModels = GetIDSCrossCheckDataInDB(criteria);

            if (data.FullDataViewModels.Count() > 0)
            {
                data.ComparingCasesViewModels = new List<PatIDSCrossCheckDataViewModel>();
                List<int> ComparingAppIds = data.FullDataViewModels.Select(c => c.AppID).Distinct().ToList();
                foreach (int AppID in ComparingAppIds)
                {
                    PatIDSCrossCheckDataViewModel viewModel = data.FullDataViewModels.FirstOrDefault(c => c.AppID == AppID);
                    data.ComparingCasesViewModels.Add(viewModel);
                }
                data.BaseCaseViewModel = data.FullDataViewModels.FirstOrDefault();
            }

            foreach (PropertyInfo propertyInfo in criteria.GetType().GetProperties())
            {
                foreach (PropertyInfo propertyInfoData in criteria.GetType().GetProperties())
                {
                    if (propertyInfoData.Name.Equals(propertyInfo.Name))
                    {
                        propertyInfoData.SetValue(data, propertyInfo.GetValue(criteria, null));
                        break;
                    }
                }
            }

            return data;
        }

        private IList<PatIDSCrossCheckDataViewModel> GetIDSCrossCheckDataInDB(PatIDSCrossCheckViewModel criteria)
        {
            IList<PatIDSCrossCheckDataViewModel> result = new List<PatIDSCrossCheckDataViewModel>();
            using (SqlCommand cmd = new SqlCommand("procWebPatIDSCrossCheck"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(_repository.Database.GetDbConnection().ConnectionString);
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                criteria.ReportFormat = 4;
                foreach (PropertyInfo propertyInfo in criteria.GetType().GetProperties())
                {
                    if (propertyInfo.Name.Equals("LanguageCode"))
                        continue;
                    if (propertyInfo.PropertyType.Name.Equals("String"))
                    {
                        var value = propertyInfo.GetValue(criteria, null);
                        if (value != null)
                            propertyInfo.SetValue(criteria, value.ToString().Replace('*', '%').Replace('?', '_'));
                        else
                        {
                            if (propertyInfo.Name.EndsWith("Op"))
                            {
                                propertyInfo.SetValue(criteria, "eq");
                            }
                        }
                    }
                    cmd.Parameters.AddWithValue("@" + propertyInfo.Name, propertyInfo.GetValue(criteria, null));

                }
                cmd.Parameters.AddWithValue("@UserID", _httpContextAccessor.HttpContext.User.GetUserIdentifier());

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    PatIDSCrossCheckDataViewModel viewModel = new PatIDSCrossCheckDataViewModel();
                    if (reader["BaseAppId"] != DBNull.Value)
                    {
                        viewModel.BaseAppId = (int)reader["BaseAppId"];
                        viewModel.BaseCaseNumber = reader["BaseCaseNumber"] != DBNull.Value ? (string)reader["BaseCaseNumber"] : null;
                        viewModel.BaseCountry = reader["BaseCountry"] != DBNull.Value ? (string)reader["BaseCountry"] : null;
                        viewModel.BaseSubCase = reader["BaseSubCase"] != DBNull.Value ? (string)reader["BaseSubCase"] : null;
                        viewModel.BaseCaseType = reader["BaseCaseType"] != DBNull.Value ? (string)reader["BaseCaseType"] : null;
                        viewModel.RowID = (int)reader["RowID"];
                        viewModel.AppID = (int)reader["AppID"];
                        viewModel.RelatedCasesId = (int)reader["RelatedCasesId"];
                        viewModel.CaseNumber = reader["CaseNumber"] != DBNull.Value ? (string)reader["CaseNumber"] : null;
                        viewModel.Country = reader["Country"] != DBNull.Value ? (string)reader["Country"] : null;
                        viewModel.SubCase = reader["SubCase"] != DBNull.Value ? (string)reader["SubCase"] : null;
                        viewModel.RelCountries = reader["RelCountries"] != DBNull.Value ? (string)reader["RelCountries"] : null;
                        viewModel.CaseType = reader["CaseType"] != DBNull.Value ? (string)reader["CaseType"] : null;
                        viewModel.RelPubNo = reader["RelPubNo"] != DBNull.Value ? (string)reader["RelPubNo"] : null;
                        viewModel.RelPubDate = reader["RelPubDate"] == DBNull.Value ? null : (DateTime?)reader["RelPubDate"];
                        viewModel.RelPatNo = reader["RelPatNo"] != DBNull.Value ? (string)reader["RelPatNo"] : null;
                        viewModel.RelIssDate = reader["RelIssDate"] == DBNull.Value ? null : (DateTime?)reader["RelIssDate"];
                        viewModel.CitedInMaster = (bool)reader["CitedInMaster"];
                        viewModel.CitedInCompare = (bool)reader["CitedInCompare"];
                        viewModel.RelatedDateFiledInMaster = reader["RelatedDateFiledInMaster"] == DBNull.Value ? null : (DateTime?)reader["RelatedDateFiledInMaster"];
                        viewModel.RelatedDateFiledInCompare = reader["RelatedDateFiledInCompare"] == DBNull.Value ? null : (DateTime?)reader["RelatedDateFiledInCompare"];
                        viewModel.ActiveSwitchInMaster = (bool)reader["ActiveSwitchInMaster"];
                        viewModel.ActiveSwitchInCompare = (bool)reader["ActiveSwitchInCompare"];
                    }
                    result.Add(viewModel);
                }

                return result;
            }
        }

        public class IDSCrossCheckData : PatIDSCrossCheckViewModel
        {
            public IList<PatIDSCrossCheckDataViewModel> FullDataViewModels { get; set; }
            public IList<PatIDSCrossCheckDataViewModel> ComparingCasesViewModels { get; set; }
            public PatIDSCrossCheckDataViewModel BaseCaseViewModel { get; set; }
        }


        // Inventor Awards Report
        public class PatInventorAwardsReportData : PatInventorAwardsReportViewModel
        {
            public IList<PatInventorAwardsReportDataViewModel>? AwardData { get; set; }
        }

        public PatInventorAwardsReportData GetInventorAwardsData(PatInventorAwardsReportViewModel criteria)
        {
            PatInventorAwardsReportData data = new PatInventorAwardsReportData();
            data.AwardData = GetInventorAwardsDataInDB(criteria);

            if (data.AwardData.Count() > 0)
            {
                var primaryGroup = criteria.SortOrder switch
                {
                    1 => data.AwardData.GroupBy(a => (object)a.Inventor).OrderBy(g => g.Key).ToList(),
                    2 => data.AwardData.GroupBy(a => (object)a.AwardDate).OrderBy(g => g.Key).ToList(),
                    3 => data.AwardData.GroupBy(a => (object)a.PaymentDate).OrderBy(g => g.Key).ToList(),
                    _ => throw new ArgumentException("Invalid grouping parameter.")
                };

                data.AwardData = new List<PatInventorAwardsReportDataViewModel>();

                foreach (var primaryGroupItem in primaryGroup)
                {
                    if (primaryGroupItem.Key == null || string.IsNullOrEmpty(primaryGroupItem.Key.ToString()))
                        continue;

                    var formattedKey = primaryGroupItem.Key switch
                    {
                        DateTime date => date.ToString("dd-MMM-yyyy"),
                        _ => primaryGroupItem.Key.ToString() ?? ""
                    };

                    var inventorAwardGroup = new PatInventorAwardsReportDataViewModel
                    {
                        SortKey = formattedKey,
                        Cases = new List<CaseGroupViewModel>(),
                        SortField = criteria.SortOrder switch
                        {
                            1 => "Inventor",
                            2 => "Award Date",
                            3 => "Payment Date",
                            _ => ""
                        },
                        TotalAmount = primaryGroupItem.Sum(a => a.Amount ?? 0)

                    };

                    var caseNumberGroup = primaryGroupItem.GroupBy(a => new { a.CaseNumber , a.Country, a.SubCase})
                                          .OrderBy(g => g.Key.CaseNumber).ThenBy(g => g.Key.Country).ThenBy(g => g.Key.SubCase).ToList();

                    foreach (var caseGroupItem in caseNumberGroup)
                    {
                        var awardsForCase = caseGroupItem.OrderBy(g => g.Inventor).ThenBy(g => g.AwardDate).ToList();

                        var caseGroupViewModel = new CaseGroupViewModel
                        {
                            CaseNumber = caseGroupItem.Key.CaseNumber,
                            Country = caseGroupItem.Key.Country,
                            SubCase = caseGroupItem.Key.SubCase,
                            Awards = awardsForCase
                        };

                        inventorAwardGroup.Cases.Add(caseGroupViewModel);
                    }
                    data.AwardData.Add(inventorAwardGroup);
                }
            }
            return data;
        }

        private IList<PatInventorAwardsReportDataViewModel> GetInventorAwardsDataInDB(PatInventorAwardsReportViewModel criteria)
        {
            IList<PatInventorAwardsReportDataViewModel> result = new List<PatInventorAwardsReportDataViewModel>();
            using (SqlCommand cmd = new SqlCommand("procWebPatInventorAwardReport"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(_repository.Database.GetDbConnection().ConnectionString);
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                criteria.ReportFormat = 4;
                foreach (PropertyInfo propertyInfo in criteria.GetType().GetProperties())
                {
                    if (propertyInfo.Name.Equals("LanguageCode") || propertyInfo.Name.Equals("PrintShowCriteriaOnFirstPage"))
                        continue;
                    if (propertyInfo.PropertyType.Name.Equals("String"))
                    {
                        var value = propertyInfo.GetValue(criteria, null);
                        if (value != null)
                            propertyInfo.SetValue(criteria, value.ToString().Replace('*', '%').Replace('?', '_'));
                        else
                        {
                            if (propertyInfo.Name.EndsWith("Op"))
                            {
                                propertyInfo.SetValue(criteria, "eq");
                            }
                        }
                    }
                    cmd.Parameters.AddWithValue("@" + propertyInfo.Name, propertyInfo.GetValue(criteria, null));
                }
                cmd.Parameters.AddWithValue("@UserID", _httpContextAccessor.HttpContext.User.GetUserIdentifier());

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    PatInventorAwardsReportDataViewModel viewModel = new PatInventorAwardsReportDataViewModel();
                    if (reader["AwardId"] != DBNull.Value)
                    {
                        viewModel.AwardId = (int)reader["AwardId"];
                        viewModel.AwardSource = reader["AwardSource"] != DBNull.Value ? (string)reader["AwardSource"] : null;
                        viewModel.AppId = reader["AppId"] != DBNull.Value ? (int)reader["AppId"] : null;
                        viewModel.DMSId = reader["DMSId"] !=  DBNull.Value ? (int)reader["DMSId"] : null;
                        viewModel.InventorId = (int)reader["InventorId"];
                        viewModel.Inventor = reader["Inventor"] != DBNull.Value ? (string)reader["Inventor"] : null;
                        viewModel.CaseNumber = reader["CaseNumber"] != DBNull.Value ? (string)reader["CaseNumber"] : null;
                        viewModel.Country = reader["Country"] != DBNull.Value ? (string)reader["Country"] : null;
                        viewModel.SubCase = reader["SubCase"] != DBNull.Value ? (string)reader["SubCase"] : null;
                        viewModel.CaseType = reader["CaseType"] != DBNull.Value ? (string)reader["CaseType"] : null;
                        viewModel.ApplicationStatus = reader["ApplicationStatus"] != DBNull.Value ? (string)reader["ApplicationStatus"] : null;
                        viewModel.ApplicationStatusDate = reader["ApplicationStatusDate"] == DBNull.Value ? null : (DateTime?)reader["ApplicationStatusDate"];
                        viewModel.AppNumber = reader["AppNumber"] != DBNull.Value ? (string)reader["AppNumber"] : null;
                        viewModel.FilDate = reader["FilDate"] == DBNull.Value ? null : (DateTime?)reader["FilDate"];
                        viewModel.PatNumber = reader["PatNumber"] != DBNull.Value ? (string)reader["PatNumber"] : null;
                        viewModel.IssDate = reader["IssDate"] == DBNull.Value ? null : (DateTime?)reader["IssDate"];
                        viewModel.ExpDate = reader["ExpDate"] == DBNull.Value ? null : (DateTime?)reader["ExpDate"];
                        viewModel.Amount = (decimal)reader["Amount"];
                        viewModel.AwardType = reader["AwardType"] != DBNull.Value ? (string)reader["AwardType"] : null;
                        viewModel.AwardDate = reader["AwardDate"] == DBNull.Value ? null : (DateTime?)reader["AwardDate"];
                        viewModel.PaymentDate = reader["PaymentDate"] == DBNull.Value ? null : (DateTime?)reader["PaymentDate"];
                        viewModel.Remarks = reader["Remarks"] != DBNull.Value ? (string)reader["Remarks"] : null;
                        viewModel.LastUpdate = reader["LastUpdate"]  == DBNull.Value ? null : (DateTime?)reader["LastUpdate"];
                    }
                    result.Add(viewModel);
                }
                return result;
            }
        }
    }
}
