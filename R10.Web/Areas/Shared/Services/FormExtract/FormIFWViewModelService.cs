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

        public FormIFWViewModelService(ICountryApplicationService applicationService, IFormIFWService ifwService)
        {
            _applicationService = applicationService;
            _ifwService = ifwService;
        }

        public IQueryable<CountryApplication> CtryAppsWithIFW
        {
            get
            {
                var ctryApps = _applicationService.CountryApplications;
                return ctryApps;
            }
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
