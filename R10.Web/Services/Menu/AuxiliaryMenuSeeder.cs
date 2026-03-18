using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace R10.Web.Services.Menu
{
    /// <summary>
    /// Seeds Auxiliary menu items under Patent and Trademark in the MegaMenu on app startup.
    /// Idempotent — checks existence before creating any records.
    /// </summary>
    public class AuxiliaryMenuSeeder : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuxiliaryMenuSeeder> _logger;

        public AuxiliaryMenuSeeder(IServiceScopeFactory scopeFactory, ILogger<AuxiliaryMenuSeeder> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ICPiDbContext>();

                var menuItemRepo = db.GetRepository<CPiMenuItem>();
                var menuPageRepo = db.GetRepository<CPiMenuPage>();

                var allItems = await menuItemRepo.QueryableList.Include(m => m.Page).ToListAsync(cancellationToken);
                var allPages = await menuPageRepo.QueryableList.ToListAsync(cancellationToken);

                // Find top-level Patent and Trademark menu items
                var patentTop = allItems.FirstOrDefault(m => m.ParentId == "" && m.Title == "Patent");
                var trademarkTop = allItems.FirstOrDefault(m => m.ParentId == "" && m.Title == "Trademark");

                if (patentTop == null || trademarkTop == null)
                {
                    _logger.LogWarning("AuxiliaryMenuSeeder: Could not find Patent or Trademark top-level menu items. Skipping seed.");
                    return;
                }

                // Patent auxiliary items
                var patentItems = new List<(string Title, string Controller, string RouteOptions, int SortOrder)>
                {
                    ("Action Type", "ActionType", "{\"area\":\"Patent\"}", 10),
                    ("Area", "Area", "{\"area\":\"Patent\"}", 20),
                    ("Country Due", "CountryDue", "{\"area\":\"Patent\"}", 30),
                    ("Country Expiry", "CountryExp", "{\"area\":\"Patent\"}", 40),
                    ("Des Case Type", "DesCaseType", "{\"area\":\"Patent\"}", 50),
                    ("Des Case Type Fields", "DesCaseTypeFields", "{\"area\":\"Patent\"}", 60)
                };

                // Trademark auxiliary items
                var trademarkItems = new List<(string Title, string Controller, string RouteOptions, int SortOrder)>
                {
                    ("Action Type", "ActionType", "{\"area\":\"Trademark\"}", 10),
                    ("Area", "Area", "{\"area\":\"Trademark\"}", 20),
                    ("Country Due", "CountryDue", "{\"area\":\"Trademark\"}", 30),
                    ("Des Case Type", "DesCaseType", "{\"area\":\"Trademark\"}", 40),
                    ("Des Case Type Fields", "DesCaseTypeFields", "{\"area\":\"Trademark\"}", 50),
                    ("Standard Good", "StandardGood", "{\"area\":\"Trademark\"}", 60)
                };

                bool changed = false;

                changed |= await SeedAuxiliaryCategory(db, menuItemRepo, menuPageRepo, allItems, allPages, patentTop.Id, "Patent", patentItems, cancellationToken);
                changed |= await SeedAuxiliaryCategory(db, menuItemRepo, menuPageRepo, allItems, allPages, trademarkTop.Id, "Trademark", trademarkItems, cancellationToken);

                if (changed)
                {
                    _logger.LogInformation("AuxiliaryMenuSeeder: Menu items seeded. Restart or wait for cache expiration to see changes.");
                }
                else
                {
                    _logger.LogInformation("AuxiliaryMenuSeeder: All auxiliary menu items already exist.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuxiliaryMenuSeeder: Error seeding auxiliary menu items.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task<bool> SeedAuxiliaryCategory(
            ICPiDbContext db,
            IRepository<CPiMenuItem> menuItemRepo,
            IRepository<CPiMenuPage> menuPageRepo,
            List<CPiMenuItem> allItems,
            List<CPiMenuPage> allPages,
            string parentTopId,
            string areaName,
            List<(string Title, string Controller, string RouteOptions, int SortOrder)> items,
            CancellationToken cancellationToken)
        {
            bool changed = false;

            // Find or create "Auxiliary" category
            var auxCategory = allItems.FirstOrDefault(m => m.ParentId == parentTopId && m.Title == "Auxiliary");
            if (auxCategory != null && string.IsNullOrEmpty(auxCategory.Policy))
            {
                // Fix previously seeded items missing Policy
                auxCategory.Policy = "*";
                menuItemRepo.Update(auxCategory);
                await db.SaveChangesAsync();
                _logger.LogInformation("AuxiliaryMenuSeeder: Fixed missing Policy on 'Auxiliary' category under {Area}.", areaName);
                changed = true;
            }
            if (auxCategory == null)
            {
                auxCategory = new CPiMenuItem
                {
                    ParentId = parentTopId,
                    Title = "Auxiliary",
                    SortOrder = 20,
                    IsEnabled = true,
                    Policy = "*"
                };
                menuItemRepo.Add(auxCategory);
                await db.SaveChangesAsync();
                _logger.LogInformation("AuxiliaryMenuSeeder: Created 'Auxiliary' category under {Area}.", areaName);
                changed = true;
            }

            foreach (var (title, controller, routeOptions, sortOrder) in items)
            {
                // Find or create the page
                var page = allPages.FirstOrDefault(p => p.Controller == controller && p.RouteOptions == routeOptions);
                if (page == null)
                {
                    page = new CPiMenuPage
                    {
                        Name = $"{areaName} {title}",
                        Controller = controller,
                        Action = "Index",
                        RouteOptions = routeOptions,
                        Policy = "*"
                    };
                    menuPageRepo.Add(page);
                    await db.SaveChangesAsync();
                    allPages.Add(page);
                    _logger.LogInformation("AuxiliaryMenuSeeder: Created page '{PageName}'.", page.Name);
                    changed = true;
                }

                // Find or create the leaf menu item
                var leafItem = allItems.FirstOrDefault(m => m.ParentId == auxCategory.Id && m.PageId == page.Id);
                if (leafItem != null && (string.IsNullOrEmpty(leafItem.Policy) || !leafItem.IsEnabled))
                {
                    // Fix previously seeded items missing Policy or disabled
                    if (string.IsNullOrEmpty(leafItem.Policy)) leafItem.Policy = "*";
                    if (!leafItem.IsEnabled) leafItem.IsEnabled = true;
                    menuItemRepo.Update(leafItem);
                    await db.SaveChangesAsync();
                    _logger.LogInformation("AuxiliaryMenuSeeder: Fixed Policy/IsEnabled on '{Title}' under {Area}/Auxiliary.", title, areaName);
                    changed = true;
                }
                if (leafItem == null)
                {
                    leafItem = new CPiMenuItem
                    {
                        ParentId = auxCategory.Id,
                        Title = title,
                        PageId = page.Id,
                        SortOrder = sortOrder,
                        IsEnabled = true,
                        Policy = "*"
                    };
                    menuItemRepo.Add(leafItem);
                    await db.SaveChangesAsync();
                    allItems.Add(leafItem);
                    _logger.LogInformation("AuxiliaryMenuSeeder: Created menu item '{Title}' under {Area}/Auxiliary.", title, areaName);
                    changed = true;
                }
            }

            return changed;
        }
    }
}
