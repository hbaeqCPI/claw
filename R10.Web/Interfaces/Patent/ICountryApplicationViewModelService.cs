using Kendo.Mvc.UI;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface ICountryApplicationViewModelService
    {
        IQueryable<CountryApplication> AddCriteria(IQueryable<CountryApplication> applications, List<QueryFilterViewModel> mainSearchFilters);
        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<CountryApplication> applications);
        Task<CountryApplicationDetailViewModel> CreateViewModelForDetailScreen(int id);
        Task<List<InventionCountryApplicationViewModel>> GetInventionCountryApplications(int invId);
        Task<List<PatIDSMassCopyFamilyDTO>> GetRelatedApplications(int invId, int appId, string relatedBy,bool activeOnly);

        IQueryable<CaseNumberLookupViewModel> GetCaseNumbersList(IQueryable<CountryApplication> countryApplications,
            DataSourceRequest request, string textProperty, string text, FilterType filterType);

        Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<CountryApplication> countryApplications,
            string value);
        Task<double> GetPatentScore(int appId);
        Task<List<PatScoreDTO>> GetPatentScores(int appId);
        Task<List<PatScoreCategory>> GetPatentScoreCategories();

        #region Family Tree View
        Task<FamilyTreeDiagram> GetFamilyTreeDiagram(string paramType, string paramValue);
        Task<FamilyTreeDiagram> GetTerminalDisclaimerDiagram(int terminalDisclaimerAppId);
        Task<IEnumerable<FamilyTreeDTO> > GetFamilyTree(string paramType, string paramValue, string paramParent);
        FamilyTreePatDTO GetNodeDetails(string paramType, string paramValue);
        void UpdateParent(int childAppId, int newParentId, string parentInfo, string userName);

        Task<List<SysCustomFieldSetting>> GetCustomFields();
        Task<List<SysCustomFieldSetting>> GetInventionCustomFieldsForSearch();
        #endregion

        Task<List<WorkflowEmailViewModel>> ProcessSaveWorkflow(CountryApplication app, bool checkStatusChangeWorkFlow, string? oldApplicationStatus, string? emailUrl, string? userName,string? actionEmailUrl,bool checkDisclosureStatusChangeWorkFlow, 
                                                               string? oldDisclosureStatus, string? disclosureStatusChangeEmailUrl);

        Task ApplyDetailPageTradeSecretPermission(DetailPageViewModel<CountryApplicationDetailViewModel> viewModel);
    }
}


