using Kendo.Mvc;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.Services
{
    public class FormIFWViewModelService : IFormIFWViewModelService
    {
        private readonly ICountryApplicationService _applicationService;
        private readonly IFormIFWService _ifwService;
        private readonly IRTSService _rtsService;

        public FormIFWViewModelService(ICountryApplicationService applicationService, IFormIFWService ifwService, IRTSService rtsService)
        {
            _applicationService = applicationService;
            _ifwService = ifwService;
            _rtsService = rtsService;
        }

        public IQueryable<CountryApplication> CtryAppsWithIFW
        {
            get
            {
                var ctryApps = _applicationService.CountryApplications;
                ctryApps = ctryApps.Where(c => c.RTSSearch.RTSSearchUSIFWs.Any());
                return ctryApps;
            }
        }


        public IQueryable<RTSSearchUSIFW> AddCriteria(IQueryable<RTSSearchUSIFW> ifws, List<QueryFilterViewModel> mainSearchFilters)
        {
            
            if (mainSearchFilters.Count > 0)
            {

                var caseTypeOp = mainSearchFilters.GetFilterOperator("CaseTypeOp");
                var caseType = mainSearchFilters.FirstOrDefault(f => f.Property.EndsWith(".CaseType"));
                if (caseType != null)
                {
                    caseType.Operator = caseTypeOp;
                    var caseTypes = caseType.GetValueList();

                    if (caseTypes.Count > 0)
                    {
                        if (caseType.Operator == "eq")
                            ifws = ifws.Where(m => caseTypes.Contains(m.RTSSearch.CountryApplication.CaseType));
                        else
                            ifws = ifws.Where(m => !caseTypes.Contains(m.RTSSearch.CountryApplication.CaseType));

                        mainSearchFilters.Remove(caseType);
                    }
                }

                var applicationStatusOp = mainSearchFilters.GetFilterOperator("ApplicationStatusOp");
                var applicationStatus = mainSearchFilters.FirstOrDefault(f => f.Property.EndsWith(".ApplicationStatus"));
                if (applicationStatus != null)
                {
                    applicationStatus.Operator = applicationStatusOp;
                    var statuses = applicationStatus.GetValueList();
                    if (statuses.Count > 0)
                    {
                        if (applicationStatus.Operator == "eq")
                            ifws = ifws.Where(m => statuses.Contains(m.RTSSearch.CountryApplication.ApplicationStatus));
                        else
                            ifws = ifws.Where(m => !statuses.Contains(m.RTSSearch.CountryApplication.ApplicationStatus));

                        mainSearchFilters.Remove(applicationStatus);
                    }
                }

                var aiParsedStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "AIParsed");
                if (aiParsedStatus != null)
                {
                    switch (aiParsedStatus.Value) {
                        case "true": ifws = ifws.Where(m => m.AIParseDate != null); break;
                        case "false": ifws = ifws.Where(m => m.AIParseDate == null); break;
                    }
                    mainSearchFilters.Remove(aiParsedStatus);
                }

                var actionGenStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "ActionGen");
                if (actionGenStatus != null)
                {
                    switch (actionGenStatus.Value)
                    {
                        case "true": ifws = ifws.Where(m => m.AIActionGenDate != null); break;
                        case "false": ifws = ifws.Where(m => m.AIActionGenDate == null); break;
                    }
                    mainSearchFilters.Remove(actionGenStatus);
                }

                var includeStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "IsIncluded");
                if (includeStatus != null)
                {
                    switch (includeStatus.Value)
                    {
                        case "true": ifws = ifws.Where(m => (bool) m.AIInclude); break;
                        case "false": ifws = ifws.Where(m => ! (bool)m.AIInclude); break;
                    }
                    mainSearchFilters.Remove(includeStatus);
                }

                if (mainSearchFilters.Any())
                {
                    ifws = QueryHelper.BuildCriteria<RTSSearchUSIFW>(ifws, mainSearchFilters);
                }
            }
            return ifws;
        }


        public IQueryable<FormIFWFormTypeViewModel> GetIFWFormTypeList(string textProperty, string text, FilterType filterType)
        {
            var formTypes = _ifwService.FormIFWFormTypes;
            formTypes = QueryHelper.BuildCriteria(formTypes, textProperty, text, filterType);

            var result = formTypes.Select(d => new FormIFWFormTypeViewModel { FormType = d.FormType, FormName = d.FormName }).OrderBy(a => a.FormName);
            return result;
        }

        public IQueryable<FormIFWDocTypeViewModel> GetIFWDocTypeList(string textProperty, string text, FilterType filterType)
        {
            var docTypes = _ifwService.FormIFWDocTypes;
            docTypes = QueryHelper.BuildCriteria(docTypes, textProperty, text, filterType);

            var result = docTypes.Select(d => new FormIFWDocTypeViewModel { DocTypeId = d.DocTypeId, DocDesc= d.DocDesc}).OrderBy(a => a.DocDesc);
            return result;
        }

        public async Task<string> GetDetailViewAsync(string formType)
        {
            var view = await _ifwService.FormIFWFormTypes.Where(f => f.FormType == formType).Select(f => f.DetailView).FirstOrDefaultAsync();
            return view;
        }

        public async Task<FormIFWActionMap> CreateActionMapEditorViewModel(int docTypeId, int mapId)
        {
            var model = new FormIFWActionMap();
            if (mapId > 0)
            {
                model = await _ifwService.FormIFWActionMaps.FirstOrDefaultAsync(a => a.MapId == mapId);
            }
            else
                model.DocTypeId = docTypeId;

            return model;
        }

     
    }
}
