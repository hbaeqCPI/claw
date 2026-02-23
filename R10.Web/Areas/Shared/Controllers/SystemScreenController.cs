using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class SystemScreenController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<SystemScreen> _viewModelService;
        private readonly IAsyncRepository<SystemScreen> _repository;
        private readonly ISystemSettings<PatSetting> _patSettings;

        public SystemScreenController(IAuthorizationService authService, IViewModelService<SystemScreen> viewModelService, IAsyncRepository<SystemScreen> repository,
            ISystemSettings<PatSetting> patSettings)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _repository = repository;
            _patSettings = patSettings;
        }

        public IActionResult GetScreenBySystem(string featureType, string systemType)
        {
            var screens = _repository.QueryableList;
            var result = screens.Where(s => s.SystemType == systemType && s.FeatureType == featureType)
                                           .Select(s => new { ScreenId = s.ScreenId, ScreenName = s.ScreenName }).OrderBy(s => s.ScreenName);
            var list = result.ToList();

            if (systemType == "P")
            {
                if (!_patSettings.GetSetting().Result.IsInventorRemunerationOn)
                {
                    list.RemoveAll(c => c.ScreenName== "Distribution Award" || c.ScreenName == "Lump Sum Award" || c.ScreenName == "Yearly Award");
                }
                if (!_patSettings.GetSetting().Result.IsInventorFRRemunerationOn)
                {
                    list.RemoveAll(c => c.ScreenName == "French Remuneration");
                }
                if (!_patSettings.GetSetting().Result.IsInventionActionOn)
                {
                    list.RemoveAll(c => (c.ScreenName.Contains("Invention Action") || c.ScreenName.Contains("Invention Due") || c.ScreenName.Contains("Invention DeDocket Instruction")));
                }
                if (!_patSettings.GetSetting().Result.IsInventionCostTrackingOn)
                {
                    list.RemoveAll(c => (c.ScreenName.Contains("Invention Cost")));
                }
            }

            return Json(list);    
        }

    }
}
