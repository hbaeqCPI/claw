using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Security;
using R10.Web.Extensions;
using R10.Web.Interfaces;
using AutoMapper.QueryableExtensions;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Models.PageViewModels;
using AutoMapper;
using R10.Core.Services;
// using R10.Core.Entities.DMS; // Removed during deep clean
using R10.Core.Helpers;
using OpenIddict.Validation.AspNetCore;
using R10.Core.Interfaces.Patent;
using R10.Core.Interfaces.Trademark;
using R10.Core.Entities.Trademark;
using R10.Core.Entities;
using R10.Core.Identity;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize]
    public class SoftDocketController : BaseController
    {
        private readonly ISoftDocketService _softDocketService;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly IAuthorizationService _authService;
        private readonly IAttorneyService _attorneyService;

        public SoftDocketController(ISoftDocketService softDocketService, ISystemSettings<PatSetting> patSettings, ISystemSettings<TmkSetting> tmkSettings, 
            IAuthorizationService authService, IAttorneyService attorneyService)
        {
            _softDocketService = softDocketService;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _authService = authService;
            _attorneyService = attorneyService;
        }

        public async Task<IActionResult> SoftDocketEntry(string system, int parentId,bool fromInvention=false)
        {
            var model = new SoftDocketViewModel();
            model.SystemType = system;
            model.ParentId = parentId;

            var isAttorneyUser = User.GetUserType() == CPiUserType.Attorney;
            if (isAttorneyUser)
            {
                var attorney = await _attorneyService.GetByIdAsync(User.GetEntityId() ?? 0);
                if (attorney != null)
                {
                    model.ResponsibleId = attorney.AttorneyID;
                    model.Responsible = attorney.AttorneyCode;
                }
            }

            switch (system) {
                case SystemTypeCode.Patent:
                    if (!(await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.SoftDocketAdd)).Succeeded)
                        return Forbid();

                    var patSettings = await _patSettings.GetSetting();
                    if (fromInvention)
                    {
                        model.Controller = "ActionDueInv";
                        model.HasCountry = false;
                        model.HasSubCase = false;
                        model.HasCaseType = false;
                        model.HasStatus = true;

                        var inv = await _softDocketService.GetInvention(parentId);
                        if (inv != null)
                        {
                            model.Area = "Patent";
                            model.CaseNumber = inv.CaseNumber;
                            model.Status = inv.DisclosureStatus;
                            model.Title = inv.InvTitle;

                            if (!isAttorneyUser)
                                switch (patSettings.SoftDocketDefaultResponsibleAtty.ToLower()) {
                                    case "attorney1":
                                        if (inv.Attorney1 !=null) {
                                            model.ResponsibleId = inv.Attorney1ID;
                                            model.Responsible = inv.Attorney1.AttorneyCode;
                                        }
                                        break;
                                    case "attorney2":
                                        if (inv.Attorney2 != null)
                                        {
                                            model.ResponsibleId = inv.Attorney2ID;
                                            model.Responsible = inv.Attorney2.AttorneyCode;
                                        }
                                        break;
                                    case "attorney3":
                                        if (inv.Attorney3 != null)
                                        {
                                            model.ResponsibleId = inv.Attorney3ID;
                                            model.Responsible = inv.Attorney3.AttorneyCode;
                                        }
                                        break;
                                    case "attorney4":
                                        if (inv.Attorney4 != null)
                                        {
                                            model.ResponsibleId = inv.Attorney4ID;
                                            model.Responsible = inv.Attorney4.AttorneyCode;
                                        }
                                        break;
                                    case "attorney5":
                                        if (inv.Attorney5 != null)
                                        {
                                            model.ResponsibleId = inv.Attorney5ID;
                                            model.Responsible = inv.Attorney5.AttorneyCode;
                                        }
                                        break;
                                }
                        }
                    }
                    else {
                        var app = await _softDocketService.GetApplication(parentId);
                        if (app != null)
                        {
                            model.Area = "Patent";
                            model.CaseNumber = app.CaseNumber;
                            model.Country = app.Country;
                            model.SubCase = app.SubCase;
                            model.CaseType = app.CaseType;
                            model.Status = app.ApplicationStatus;
                            model.Title = app.AppTitle;

                            if (!isAttorneyUser)
                                switch (patSettings.SoftDocketDefaultResponsibleAtty.ToLower())
                                {
                                    case "attorney1":
                                        if (app.Invention.Attorney1 != null)
                                        {
                                            model.ResponsibleId = app.Invention.Attorney1ID;
                                            model.Responsible = app.Invention.Attorney1.AttorneyCode;
                                        }
                                        break;
                                    case "attorney2":
                                        if (app.Invention.Attorney2 != null)
                                        {
                                            model.ResponsibleId = app.Invention.Attorney2ID;
                                            model.Responsible = app.Invention.Attorney2.AttorneyCode;
                                        }
                                        break;
                                    case "attorney3":
                                        if (app.Invention.Attorney3 != null)
                                        {
                                            model.ResponsibleId = app.Invention.Attorney3ID;
                                            model.Responsible = app.Invention.Attorney3.AttorneyCode;
                                        }
                                        break;
                                    case "attorney4":
                                        if (app.Invention.Attorney4 != null)
                                        {
                                            model.ResponsibleId = app.Invention.Attorney4ID;
                                            model.Responsible = app.Invention.Attorney4.AttorneyCode;
                                        }
                                        break;
                                    case "attorney5":
                                        if (app.Invention.Attorney5 != null)
                                        {
                                            model.ResponsibleId = app.Invention.Attorney5ID;
                                            model.Responsible = app.Invention.Attorney5.AttorneyCode;
                                        }
                                        break;
                                }
                        }
                    }
                    break;

                case SystemTypeCode.Trademark:
                    if (!(await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.SoftDocketAdd)).Succeeded)
                        return Forbid();

                    var tmkSettings = await _tmkSettings.GetSetting();
                    var tmk = await _softDocketService.GetTrademark(parentId);
                    if (tmk != null)
                    {
                        model.Area = "Trademark";
                        model.CaseNumber = tmk.CaseNumber;
                        model.Country = tmk.Country;
                        model.SubCase = tmk.SubCase;
                        model.CaseType = tmk.CaseType;
                        model.Status = tmk.TrademarkStatus;
                        model.Title = tmk.TrademarkName;

                        if (!isAttorneyUser)
                            switch (tmkSettings.SoftDocketDefaultResponsibleAtty.ToLower())
                            {
                                case "attorney1":
                                    if (tmk.Attorney1 != null)
                                    {
                                        model.ResponsibleId = tmk.Attorney1ID;
                                        model.Responsible = tmk.Attorney1.AttorneyCode;
                                    }
                                    break;
                                case "attorney2":
                                    if (tmk.Attorney2 != null)
                                    {
                                        model.ResponsibleId = tmk.Attorney2ID;
                                        model.Responsible = tmk.Attorney2.AttorneyCode;
                                    }
                                    break;
                                case "attorney3":
                                    if (tmk.Attorney3 != null)
                                    {
                                        model.ResponsibleId = tmk.Attorney3ID;
                                        model.Responsible = tmk.Attorney3.AttorneyCode;
                                    }
                                    break;
                                case "attorney4":
                                    if (tmk.Attorney4 != null)
                                    {
                                        model.ResponsibleId = tmk.Attorney4ID;
                                        model.Responsible = tmk.Attorney4.AttorneyCode;
                                    }
                                    break;
                                case "attorney5":
                                    if (tmk.Attorney5 != null)
                                    {
                                        model.ResponsibleId = tmk.Attorney5ID;
                                        model.Responsible = tmk.Attorney5.AttorneyCode;
                                    }
                                    break;
                            }
                    }
                    break;

            }

            return PartialView("_SoftDocketEntry", model);
        }

        public async Task<IActionResult> GetAttorneyList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var attorneys = _attorneyService.QueryableList.Where(c => (bool)c.IsActive);
            return await GetPicklistData(attorneys, request, property, text, filterType, new string[] { "AttorneyID", "AttorneyCode", "AttorneyName" }, requiredRelation);
        }
    }


}
