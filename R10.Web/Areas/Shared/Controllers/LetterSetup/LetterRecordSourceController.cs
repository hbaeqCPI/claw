using R10.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Interfaces;
using R10.Web.Security;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessLetters)]
    public class LetterRecordSourceController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILetterService _letterService;

        public LetterRecordSourceController(ILetterService letterService)
        {
            _letterService = letterService;
        }

        public async Task<IActionResult> GetRecordSourceList(int parentId)
        {
            var recSource = await _letterService.LetterRecordSources.Where(rs => rs.LetId == parentId).ProjectTo<LetterDataSourceListViewModel>().OrderBy(rs => rs.DataSourceDescMain).ToListAsync();
                            //.Select(rs => new { SourceId = rs.SourceId, DataSourceDesMain = rs.DataSourceDescMain}).ToListAsync();
            return Json(recSource);
        }
    

    }
}
