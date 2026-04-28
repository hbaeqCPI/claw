using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using LawPortal.Core.Entities;
using LawPortal.Core.Helpers;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Extensions;
using LawPortal.Web.Extensions.ActionResults;
using LawPortal.Web.Models;
using LawPortal.Web.Security;

using LawPortal.Web.Areas;

namespace LawPortal.Web.Areas.Shared.Controllers
{
    [Area("Shared")] //, Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [Authorize] //DO NOT USE SHARED AUTH POLICY. SOME USERS MAY NOT HAVE SHARED SYSTEM/ROLE.
    public class SearchCriteriaController : BaseController
    {
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly JavaScriptEncoder _jsEncoder;

        public SearchCriteriaController(IApplicationDbContext repository, 
                                        IStringLocalizer<SharedResource> localizer,
                                        JavaScriptEncoder jsEncoder)
        {
            _repository = repository;
            _localizer = localizer;
            _jsEncoder = jsEncoder;
        }

        public IActionResult CriteriaScreen(string screen,string criteria, bool save=true)
        {
            var model = new SearchCriteria { ScreenName = _jsEncoder.Encode(screen), LoginName = User.GetEmail(), CriteriaData=criteria};
            ViewBag.Title = save ? _localizer["Save search criteria"] : _localizer["Saved criteria"];
            ViewBag.Action = save ? "Save" : "Load";
            return PartialView("_Criteria",model);
        }

        public async Task<IActionResult> GetCriteriaNames(string screen, string loginName) {
            var names = await _repository.SearchCriteria.Where(c=> c.ScreenName == screen && c.LoginName == loginName).SelectMany(c=>c.CriteriaDetails).ToListAsync();
            return Json(names);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SearchCriteriaDetail criteria) {

            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            UpdateEntityStamps(criteria, criteria.CritDtlId);

            var header = await _repository.SearchCriteria.Where(h => h.ScreenName == criteria.ScreenName && h.LoginName == User.GetEmail()).FirstOrDefaultAsync();
            if (header == null)
            {
                header = new SearchCriteria { ScreenName = criteria.ScreenName, LoginName = User.GetEmail(), CriteriaDetails=new List<SearchCriteriaDetail>() };
                UpdateEntityStamps(header, header.CriteriaId);
                header.CriteriaDetails.Add(criteria);
                _repository.SearchCriteria.Add(header);
            }
            else {
                criteria.CriteriaId = header.CriteriaId;
                var existing = await _repository.SearchCriteriaDetails.Where(d => d.SearchCriteria.ScreenName == criteria.ScreenName && d.SearchCriteria.LoginName == User.GetEmail() && d.CriteriaName == criteria.CriteriaName).FirstOrDefaultAsync();
                if (existing != null)
                    _repository.SearchCriteriaDetails.Remove(existing);

                if ((bool)criteria.IsDefault) {
                    var existingDefault = await _repository.SearchCriteriaDetails.Where(d => d.SearchCriteria.ScreenName == criteria.ScreenName && d.SearchCriteria.LoginName == User.GetEmail() && d.CriteriaName != criteria.CriteriaName && d.IsDefault==true).FirstOrDefaultAsync();
                    if (existingDefault != null) {
                        existingDefault.IsDefault = false;
                        _repository.SearchCriteriaDetails.Update(existingDefault);
                    }
                }
                
                if (existing != null)
                    _repository.SearchCriteriaDetails.Remove(existing);

                _repository.SearchCriteriaDetails.Add(criteria);
            }
            await _repository.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Load([FromBody] SearchCriteriaDetail criteria)
        {
            if (criteria.LoadType == "load")
            {
                var result = await _repository.SearchCriteriaDetails.Where(d => d.SearchCriteria.ScreenName == criteria.ScreenName && d.SearchCriteria.LoginName == User.GetEmail() && d.CriteriaName == criteria.CriteriaName).FirstOrDefaultAsync();
                if (result != null)
                    return Content(result.CriteriaData);
                
                return BadRequest(_localizer["Criteria is not on file"].ToString());
            }
            else {
                if (criteria.LoadType == "delete")
                {
                    var existing = await _repository.SearchCriteriaDetails.Where(d => d.SearchCriteria.ScreenName == criteria.ScreenName && d.SearchCriteria.LoginName == User.GetEmail() && d.CriteriaName == criteria.CriteriaName).FirstOrDefaultAsync();
                    if (existing != null) {
                        _repository.SearchCriteriaDetails.Remove(existing);
                        await _repository.SaveChangesAsync();
                    }
                }
                else if (criteria.LoadType == "update")
                {
                    SearchCriteriaDetail existing;
                    if (!string.IsNullOrEmpty(criteria.OldCriteriaName) && criteria.OldCriteriaName != criteria.CriteriaName) {
                        var dupe = await _repository.SearchCriteriaDetails.Where(d => d.SearchCriteria.ScreenName == criteria.ScreenName && d.SearchCriteria.LoginName == User.GetEmail() && d.CriteriaName == criteria.CriteriaName).FirstOrDefaultAsync();
                        if (dupe !=null)
                            return BadRequest(_localizer["Criteria Name is already on file"].ToString());

                        existing = await _repository.SearchCriteriaDetails.Where(d => d.SearchCriteria.ScreenName == criteria.ScreenName && d.SearchCriteria.LoginName == User.GetEmail() && d.CriteriaName == criteria.OldCriteriaName).FirstOrDefaultAsync();
                    }
                    else
                        existing = await _repository.SearchCriteriaDetails.Where(d => d.SearchCriteria.ScreenName == criteria.ScreenName && d.SearchCriteria.LoginName == User.GetEmail() && d.CriteriaName == criteria.CriteriaName).FirstOrDefaultAsync();

                    if (existing != null)
                    {
                        existing.CriteriaName = criteria.CriteriaName;
                        existing.IsDefault = criteria.IsDefault;

                        if ((bool)criteria.IsDefault)
                        {
                            var existingDefault = await _repository.SearchCriteriaDetails.Where(d => d.SearchCriteria.ScreenName == criteria.ScreenName && d.SearchCriteria.LoginName == User.GetEmail() && d.CriteriaName != criteria.CriteriaName && d.IsDefault == true).FirstOrDefaultAsync();
                            if (existingDefault != null) {
                                existingDefault.IsDefault = false;
                                _repository.SearchCriteriaDetails.Update(existingDefault);
                            }
                        }

                        _repository.SearchCriteriaDetails.Update(existing);
                        await _repository.SaveChangesAsync();
                    }

                }
                return Ok();
            }
            
        }

        public async Task<IActionResult> GetDefault(string screen)
        {
            var result = await _repository.SearchCriteriaDetails.Where(d => d.SearchCriteria.ScreenName == screen && d.SearchCriteria.LoginName == User.GetEmail() && d.IsDefault == true).FirstOrDefaultAsync();
            var loadSavedCriteria = HttpContext.Session.GetString("DoNotLoadSavedCriteria");
            if (!string.IsNullOrEmpty(loadSavedCriteria)) HttpContext.Session.Remove("DoNotLoadSavedCriteria");
            if (result != null && string.IsNullOrEmpty(loadSavedCriteria))
                return Content(result.CriteriaData);
            else
                return Ok();
        }

        public Microsoft.AspNetCore.Mvc.ActionResult GenericValueMapper(string value)
        {
            return Json(value);
        }
    }
}