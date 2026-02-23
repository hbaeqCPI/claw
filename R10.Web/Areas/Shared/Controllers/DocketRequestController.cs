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
using R10.Core.Entities.DMS;
using R10.Core.Helpers;
using OpenIddict.Validation.AspNetCore;
using R10.Core.Interfaces.Patent;
using R10.Core.DTOs;
using System.ComponentModel.DataAnnotations;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.Clearance;
using R10.Core.Identity;

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize]
    public class DocketRequestController : BaseController
    {
        private readonly IDocketRequestService _softDocketService;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly ISystemSettings<TmkSetting> _tmkSettings;
        private readonly IAuthorizationService _authService;
        private readonly IAttorneyService _attorneyService;

        public DocketRequestController(IDocketRequestService softDocketService, ISystemSettings<PatSetting> patSettings, ISystemSettings<TmkSetting> tmkSettings,
            IAuthorizationService authService, IAttorneyService attorneyService)
        {
            _softDocketService = softDocketService;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _authService = authService;
            _attorneyService = attorneyService;
        }

        public async Task<IActionResult> DocketRequestEntry(string system, int parentId,bool fromInvention=false,bool loadScreen=false)
        {
            var model = new DocketRequestViewModel();
            model.SystemType = system;
            model.ParentId = parentId;
            model.LoadScreen = loadScreen;

            var isAttorneyUser = User.GetUserType() == CPiUserType.Attorney;
            if (isAttorneyUser)
            {
                var attorney = await _attorneyService.GetByIdAsync(User.GetEntityId() ?? 0);
                if (attorney != null)
                {
                    model.DefaultResponsibleId = attorney.AttorneyID;
                    model.DefaultResponsible = attorney.AttorneyCode;
                }
            }

            switch (system) {
                case SystemTypeCode.Patent:
                    if (!(await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.CanRequestDocket)).Succeeded)
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
                                switch (patSettings.SoftDocketDefaultResponsibleAtty.ToLower())
                                {
                                    case "attorney1":
                                        if (inv.Attorney1 != null)
                                        {
                                            model.DefaultResponsibleId = inv.Attorney1ID;
                                            model.DefaultResponsible = inv.Attorney1.AttorneyCode;
                                        }
                                        break;
                                    case "attorney2":
                                        if (inv.Attorney2 != null)
                                        {
                                            model.DefaultResponsibleId = inv.Attorney2ID;
                                            model.DefaultResponsible = inv.Attorney2.AttorneyCode;
                                        }
                                        break;
                                    case "attorney3":
                                        if (inv.Attorney3 != null)
                                        {
                                            model.DefaultResponsibleId = inv.Attorney3ID;
                                            model.DefaultResponsible = inv.Attorney3.AttorneyCode;
                                        }
                                        break;
                                    case "attorney4":
                                        if (inv.Attorney4 != null)
                                        {
                                            model.DefaultResponsibleId = inv.Attorney4ID;
                                            model.DefaultResponsible = inv.Attorney4.AttorneyCode;
                                        }
                                        break;
                                    case "attorney5":
                                        if (inv.Attorney5 != null)
                                        {
                                            model.DefaultResponsibleId = inv.Attorney5ID;
                                            model.DefaultResponsible = inv.Attorney5.AttorneyCode;
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
                            model.RefreshCountUrl = Url.Action("RequestDocketPendingCount", "CountryApplication", new { area = model.Area, parentId = parentId });

                            if (!isAttorneyUser)
                                switch (patSettings.SoftDocketDefaultResponsibleAtty.ToLower())
                                {
                                    case "attorney1":
                                        if (app.Invention.Attorney1 != null)
                                        {
                                            model.DefaultResponsibleId = app.Invention.Attorney1ID;
                                            model.DefaultResponsible = app.Invention.Attorney1.AttorneyCode;
                                        }
                                        break;
                                    case "attorney2":
                                        if (app.Invention.Attorney2 != null)
                                        {
                                            model.DefaultResponsibleId = app.Invention.Attorney2ID;
                                            model.DefaultResponsible = app.Invention.Attorney2.AttorneyCode;
                                        }
                                        break;
                                    case "attorney3":
                                        if (app.Invention.Attorney3 != null)
                                        {
                                            model.DefaultResponsibleId = app.Invention.Attorney3ID;
                                            model.DefaultResponsible = app.Invention.Attorney3.AttorneyCode;
                                        }
                                        break;
                                    case "attorney4":
                                        if (app.Invention.Attorney4 != null)
                                        {
                                            model.DefaultResponsibleId = app.Invention.Attorney4ID;
                                            model.DefaultResponsible = app.Invention.Attorney4.AttorneyCode;
                                        }
                                        break;
                                    case "attorney5":
                                        if (app.Invention.Attorney5 != null)
                                        {
                                            model.DefaultResponsibleId = app.Invention.Attorney5ID;
                                            model.DefaultResponsible = app.Invention.Attorney5.AttorneyCode;
                                        }
                                        break;
                                }
                        }
                    }
                    break;

                case SystemTypeCode.Trademark:
                    if (!(await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CanRequestDocket)).Succeeded)
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
                        model.RefreshCountUrl = Url.Action("RequestDocketPendingCount", "TmkTrademark", new { area = model.Area, parentId = parentId });

                        if (!isAttorneyUser)
                            switch (tmkSettings.SoftDocketDefaultResponsibleAtty.ToLower()) 
                            {
                                case "attorney1":
                                    if (tmk.Attorney1 != null)
                                    {
                                        model.DefaultResponsibleId = tmk.Attorney1ID;
                                        model.DefaultResponsible = tmk.Attorney1.AttorneyCode;
                                    }
                                    break;
                                case "attorney2":
                                    if (tmk.Attorney2 != null)
                                    {
                                        model.DefaultResponsibleId = tmk.Attorney2ID;
                                        model.DefaultResponsible = tmk.Attorney2.AttorneyCode;
                                    }
                                    break;
                                case "attorney3":
                                    if (tmk.Attorney3 != null)
                                    {
                                        model.DefaultResponsibleId = tmk.Attorney3ID;
                                        model.DefaultResponsible = tmk.Attorney3.AttorneyCode;
                                    }
                                    break;
                                case "attorney4":
                                    if (tmk.Attorney4 != null)
                                    {
                                        model.DefaultResponsibleId = tmk.Attorney4ID;
                                        model.DefaultResponsible = tmk.Attorney4.AttorneyCode;
                                    }
                                    break;
                                case "attorney5":
                                    if (tmk.Attorney5 != null)
                                    {
                                        model.DefaultResponsibleId = tmk.Attorney5ID;
                                        model.DefaultResponsible = tmk.Attorney5.AttorneyCode;
                                    }
                                    break;
                            }
                    }
                    break;

                case SystemTypeCode.GeneralMatter:
                    if (!(await _authService.AuthorizeAsync(User, GeneralMatterAuthorizationPolicy.CanRequestDocket)).Succeeded)
                        return Forbid();

                    model.HasCountry = false;
                    var gm = await _softDocketService.GetMatter(parentId);
                    if (gm != null)
                    {
                        model.Area = "GeneralMatter";
                        model.CaseNumber = gm.CaseNumber;
                        model.SubCase = gm.SubCase;
                        model.CaseType = gm.MatterType;
                        model.Status = gm.MatterStatus;
                        model.Title = gm.MatterTitle;
                        model.RefreshCountUrl = Url.Action("RequestDocketPendingCount", "Matter", new { area = model.Area, parentId = parentId });
                    }

                    break;
            }

            return PartialView("_DocketRequestEntry", model);
        }

        public async Task<IActionResult> GetRequestTypes(string property, string text, FilterType filterType)
        {
            var result = new List<LookupDTO>();
            result.Add(new LookupDTO { Text = "New Docket", Value = "New Docket" });
            result.Add(new LookupDTO { Text = "Correction", Value = "Correction" });
            return Json(result);
        }

        public async Task<IActionResult> GetAttorneyList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var attorneys = _attorneyService.QueryableList.Where(c => (bool)c.IsActive);
            return await GetPicklistData(attorneys, request, property, text, filterType, new string[] { "AttorneyID", "AttorneyCode", "AttorneyName" }, requiredRelation);
        }
    }


}