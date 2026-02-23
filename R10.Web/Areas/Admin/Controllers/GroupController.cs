using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using R10.Core.Interfaces;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Areas.Admin.Views;
using R10.Web.Models.PageViewModels;
using R10.Web.Extensions;
using R10.Core.Identity;
using R10.Web.Areas.Admin.Helpers;
using Kendo.Mvc.UI;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using R10.Web.Extensions.ActionResults;
using R10.Web.Areas.Admin.ViewModels;
using Kendo.Mvc.Extensions;
using R10.Core.Helpers;
using AutoMapper;
using R10.Core.Entities;

using R10.Web.Areas;

namespace R10.Web.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = CPiAuthorizationPolicy.Administrator)]
    public class GroupController : BaseController
    {
        private readonly ICPiUserGroupManager _groupManager;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<AdminResource> _localizer;
        private readonly IQuickEmailSetupService _qeSetupservice;

        public GroupController(ICPiUserGroupManager groupManager, IMapper mapper, IStringLocalizer<AdminResource> localizer, IQuickEmailSetupService qeSetupservice)
        {
            _groupManager = groupManager;
            _mapper = mapper;
            _localizer = localizer;
            _qeSetupservice = qeSetupservice;
        }

        private string DataContainer => "adminGroupDetail";
        private string SidebarTitle => _localizer[AdminNavPages.SidebarTitle].ToString();
        private string SidebarPartialView => "_SidebarNav";
        private IQueryable<CPiGroup> CPiGroups => _groupManager.QueryableList;

        public IActionResult Index()
        {
            var model = new PageViewModel()
            {
                PageId = "adminGroupSearch",
                Title = _localizer["Groups"].ToString(),
                CanAddRecord = true
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarSearchResultsPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Groups
            };

            if (Request.IsAjax())
                return PartialView("Index", sidebarModel);

            return View(sidebarModel);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var groups = CPiGroups.AddCriteria(mainSearchFilters).ProjectTo<GroupListViewModel>();

                if (request.Sorts != null && request.Sorts.Any())
                    groups = groups.ApplySorting(request.Sorts);
                else
                    groups = groups.OrderBy(g => g.Name);

                var ids = await groups.Select(g => g.Id).ToArrayAsync();

                return Json(new CPiDataSourceResult()
                {
                    Data = await groups.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                    Total = ids.Length,
                    Ids = ids
                });
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<GroupDetailViewModel> GetGroupDetailViewModel(int id)
        {
            return await CPiGroups.Where(g => g.Id == id)
                                       .ProjectTo<GroupDetailViewModel>()
                                       .SingleOrDefaultAsync();
        }

        private async Task<DetailPageViewModel<GroupDetailViewModel>> PrepareEditScreen(int id)
        {
            var detail = await GetGroupDetailViewModel(id);
            var viewModel = new DetailPageViewModel<GroupDetailViewModel> { Detail = detail };

            if (detail != null)
            {
                viewModel.CanAddRecord = true;
                viewModel.CanEditRecord = true;
                viewModel.CanDeleteRecord = true;
                viewModel.CanPrintRecord = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = DataContainer;

                viewModel.SearchScreenUrl = Url.Action("Index");
                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });
            }
            return viewModel;
        }

        private DetailPageViewModel<GroupDetailViewModel> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<GroupDetailViewModel>
            {
                Detail = new GroupDetailViewModel() { IsEnabled = true }
            };

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = DataContainer;
            return viewModel;
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false)
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var detail = page.Detail;
            var model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Group Detail"].ToString(),
                RecordId = detail.Id,
                SingleRecord = singleRecord || !Request.IsAjax(),
                PagePermission = page,
                Data = detail
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarDetailPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Groups
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                {
                    model.Page = PageType.DetailContent;
                    return PartialView("_Index", model);
                }

                return PartialView("Index", sidebarModel);
            }

            return View("Index", sidebarModel);
        }

        public IActionResult Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            var model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["New Group"].ToString(),
                RecordId = detail.Id,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            var sidebarModel = new SidebarPageViewModel()
            {
                Title = SidebarTitle,
                PageTitle = model.Title,
                PageId = model.PageId,
                MainPartialView = "_SidebarDetailPage",
                MainViewModel = model,
                SideBarPartialView = SidebarPartialView,
                SideBarViewModel = AdminNavPages.Groups
            };

            if (!fromSearch)
            {
                model.Page = PageType.DetailContent;
                return PartialView("_Index", model);
            }

            return PartialView("Index", sidebarModel);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] GroupDetailViewModel groupDetail)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(groupDetail, groupDetail.Id);

                if (groupDetail.Id > 0)
                    await _groupManager.Update(groupDetail);
                else
                    await _groupManager.Add(groupDetail);

                //Create new QE Role Source
                var newRS = new QERoleSource()
                {
                    RoleSourceID = 0,
                    SystemType = "",
                    RoleType = "UG",
                    RoleName = groupDetail.Name,
                    SourceSQL = "UserGroup",
                    Description = groupDetail.Description,
                    OrderOfEntry = 0,
                    CreatedBy = groupDetail.UpdatedBy,
                    UpdatedBy = groupDetail.UpdatedBy,
                    DateCreated = groupDetail.LastUpdate,
                    LastUpdate = groupDetail.LastUpdate
                };
                await _qeSetupservice.AddRoleSourceAsync(newRS);

                return Json(groupDetail.Id);
            }
            else
            {
                //BadRequest(ModelState) won't display the error message properly 
                //return BadRequest(ModelState);
                return BadRequest(new { errors = ModelState.Errors() });
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var group = await _groupManager.GetByIdAsync(id);

            if (group == null)
                return new RecordDoesNotExistResult();

            //Remove from QERecipient then QERoleSource
            var qeRoleSource = await _qeSetupservice.GetQERoleSourceByNameAsync(group.Name);
            if (qeRoleSource != null && qeRoleSource.RoleType == "UG" && qeRoleSource.SourceSQL.ToLower() == "usergroup")
            {
                await _qeSetupservice.DeleteRoleSourcesAsync(new List<QERoleSource> { qeRoleSource });
            }

            group.tStamp = Convert.FromBase64String(tStamp);
            await _groupManager.Delete(group);

            return Ok();
        }

        public async Task<IActionResult> GroupUsersRead([DataSourceRequest] DataSourceRequest request, int id)
        {
            var result = await _groupManager.CPiUserGroups
                                            .Where(g => g.GroupId == id)
                                            .OrderBy(g => g.CPiUser.FirstName).ThenBy(g => g.CPiUser.LastName) //.ThenBy(g => g.CPiUser.Email)
                                            .ProjectTo<GroupUsersViewModel>().ToListAsync();

            //if (request.Sorts != null)
            //{
            //    var sortUser = request.Sorts.Where(s => s.Member == "User").FirstOrDefault();
            //    if (sortUser != null)
            //    {
            //        if (sortUser.SortDirection == Kendo.Mvc.ListSortDirection.Ascending)
            //            result = result.OrderBy(g => new { g.CPiUser.FirstName, g.CPiUser.LastName });
            //        else
            //            result = result.OrderByDescending(g => new { g.CPiUser.FirstName, g.CPiUser.LastName });

            //        request.Sorts.Remove(sortUser);
            //    }
            //}

            return Json(result.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GroupUsersUpdate(int id,
            [Bind(Prefix = "updated")] IEnumerable<GroupUsersViewModel> updated,
            [Bind(Prefix = "new")] IEnumerable<GroupUsersViewModel> added,
            [Bind(Prefix = "deleted")] IEnumerable<GroupUsersViewModel> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _groupManager.UpdateGroupUser(id, User.GetUserName(),
                    _mapper.Map<List<CPiUserGroup>>(updated),
                    _mapper.Map<List<CPiUserGroup>>(added),
                    _mapper.Map<List<CPiUserGroup>>(deleted));
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                _localizer["User has been saved successfully."].ToString() :
                _localizer["Users have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        public async Task<IActionResult> GroupUsersDelete([Bind(Prefix = "deleted")] GroupUsersViewModel deleted)
        {
            if (deleted.Id > 0)
            {
                await _groupManager.UpdateGroupUser(deleted.GroupId, User.GetUserName(), new List<CPiUserGroup>(), new List<CPiUserGroup>(), new List<CPiUserGroup>() { _mapper.Map<CPiUserGroup>(deleted) });
                return Ok(new { success = _localizer["User has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var group = await _groupManager.GetByIdAsync(id);
            if (group == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = group.CreatedBy, dateCreated = group.DateCreated, updatedBy = group.UpdatedBy, lastUpdate = group.LastUpdate, tStamp = group.tStamp });
        }

        public async Task<JsonResult> GroupUsers([DataSourceRequest] DataSourceRequest request, int groupId, string userId)
        {
            var users = await _groupManager.GetUsers()//.Where(u => u.Id == userId || !u.CPiUserGroups.Any(g => g.GroupId == groupId))
                                                      //.ProjectTo<PickListViewModel>()
                                                      .Select(u => new { Id = u.Id, Name = string.Concat(u.FirstName, " ", u.LastName), Email = u.Email })
                                                      .OrderBy(u => u.Name)
                                                      .ToListAsync();
            return Json(await users.ToDataSourceResultAsync(request));
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            var result = await GetPicklistData(CPiGroups, request, property, text, filterType, requiredRelation);
            return result;
        }
    }
}