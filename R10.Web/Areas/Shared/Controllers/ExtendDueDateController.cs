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

using R10.Web.Areas;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(AuthenticationSchemes = AuthSchemes)]
    public class ExtendDueDateController : BaseController
    {
        private readonly IDueDateExtensionService _dueDateExtensionService;
        
        private const string AuthSchemes = "Identity.Application" + "," + OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;

        public ExtendDueDateController(IDueDateExtensionService dueDateExtensionService)
        {
            _dueDateExtensionService = dueDateExtensionService;
        }

        [HttpPost]
        public async Task<IActionResult> ExtendDueDateEntry(ExtendDueDateViewModel setting)
        {
            return PartialView("_ExtendDueDateEntry", setting);
        }

        public IActionResult GetRepeatInterval(string recurrence)
        {
            var days = new int[] { 0, 1, 7, 14, 21, 30 };
            var weeks = new int[] { 0, 1, 2, 3, 4, 5, 6 };
            var months = new int[] { 0, 1, 2, 3, 4, 5, 6 };

            if (recurrence == Recurrence.Day)
                return Json(days);
            else if (recurrence == Recurrence.Week)
                return Json(weeks);
            else if (recurrence == Recurrence.Month)
                return Json(months);

            return Ok();
        }

        public IActionResult ComputeNextDueDate(DateTime currentDueDate, int? months, int? weeks, int? days)
        {
            var newDueDate = _dueDateExtensionService.ComputeNextDueDate(currentDueDate,months,weeks,days);
            return Json(new { newDueDate,newDueDateFormatted= newDueDate.ToString("dd-MMM-yyyy") });
        }

        public async Task<IActionResult> ExtendDueDate() {
            var updatedBy = User.GetUserName();
            await _dueDateExtensionService.ExtendDueDate(updatedBy);
            return Ok();
        }

    }

    public static class Recurrence {
        public static string Day = "D";
        public static string Week = "W";
        public static string Month = "M";
    }
}
