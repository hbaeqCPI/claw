using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using LawPortal.Core.DTOs;
using LawPortal.Core.Entities;
using LawPortal.Core.Identity;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Web.Services
{
    public class CPiUserDefaultPageManager : CPiUserSettingManager, ICPiUserDefaultPageManager
    {
        private readonly ICPiDbContext _cpiDbContext;

        public CPiUserDefaultPageManager(ICPiDbContext cpiDbContext) : base (cpiDbContext)
        {
            _cpiDbContext = cpiDbContext;
        }

        private IQueryable<CPiDefaultPage> CPiDefaultPages => _cpiDbContext.GetReadOnlyRepositoryAsync<CPiDefaultPage>().QueryableList;

        public async Task<List<CPiDefaultPage>> GetDefaultPages()
        {
            return await CPiDefaultPages.ToListAsync();
        }

        public async Task<DefaultPageAction> GetDefaultPage(string userId)
        {
            var userSettings = await GetUserSetting(userId, CPiSettings.DefaultPage);
            if (userSettings == null) return null;

            DefaultPage defaultPageSettings = userSettings.GetSettings<DefaultPage>();

            var defaultPage = await CPiDefaultPages.Where(page => page.Id == defaultPageSettings.DefaultPageId).FirstOrDefaultAsync();
            if (defaultPage == null) return null;

            DefaultPageAction defaultPageAction = new DefaultPageAction()
            {
                Controller = defaultPage.Controller,
                Action = defaultPage.Action,
                Route = string.IsNullOrEmpty(defaultPage.RouteOptions) ? new Dictionary<string, string>() : JObject.Parse(defaultPage.RouteOptions).ToObject<Dictionary<string, string>>(),
                PageId = defaultPage.Id,
                PagePolicy = defaultPage.Policy,
                SettingPolicy = userSettings.CPiSetting.Policy
            };

            return defaultPageAction;
        }

        public async Task<DefaultPageAction> GetDefaultPageById(int pageId)
        {
            var cpiSetting = await GetCPiSetting(CPiSettings.DefaultPage);
            if (cpiSetting == null) return null;

            var defaultPage = await CPiDefaultPages.Where(page => page.Id == pageId).FirstOrDefaultAsync();
            if (defaultPage == null) return null;

            DefaultPageAction defaultPageAction = new DefaultPageAction()
            {
                Controller = defaultPage.Controller,
                Action = defaultPage.Action,
                Route = string.IsNullOrEmpty(defaultPage.RouteOptions) ? new Dictionary<string, string>() : JObject.Parse(defaultPage.RouteOptions).ToObject<Dictionary<string, string>>(),
                PageId = defaultPage.Id,
                PagePolicy = defaultPage.Policy,
                SettingPolicy = cpiSetting.Policy
            };

            return defaultPageAction;
        }
    }
}
