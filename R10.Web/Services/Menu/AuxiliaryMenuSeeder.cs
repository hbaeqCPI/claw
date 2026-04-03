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

                // Patent categories
                var patManage = new List<(string Title, string Controller, string RouteOptions, int SortOrder)>
                {
                    ("Country Due", "CountryDue", "{\"area\":\"Patent\"}", 10),
                    ("Country Expiry", "CountryExp", "{\"area\":\"Patent\"}", 20),
                    ("Country Expiry Delete", "CountryExpDelete", "{\"area\":\"Patent\"}", 25),
                    ("Country Law", "CountryLaw", "{\"area\":\"Patent\"}", 30),
                };
                var patAuxiliary = new List<(string Title, string Controller, string RouteOptions, int SortOrder)>
                {
                    ("Action Type", "ActionType", "{\"area\":\"Patent\"}", 10),
                    ("Area", "Area", "{\"area\":\"Patent\"}", 20),
                    ("Area Delete", "AreaDelete", "{\"area\":\"Patent\"}", 21),
                    ("Area Country", "AreaCountry", "{\"area\":\"Patent\"}", 22),
                    ("Area Country Delete", "AreaCountryDelete", "{\"area\":\"Patent\"}", 23),
                    ("Case Type", "CaseType", "{\"area\":\"Patent\"}", 30),
                    ("Country", "Country", "{\"area\":\"Patent\"}", 40),
                    ("Country Law Ext", "CountryLawExt", "{\"area\":\"Patent\"}", 60),
                    ("Indicator", "Indicator", "{\"area\":\"Patent\"}", 70)
                };
                var patDesignated = new List<(string Title, string Controller, string RouteOptions, int SortOrder)>
                {
                    ("Des Case Type", "DesCaseType", "{\"area\":\"Patent\"}", 10),
                    ("Des Case Type Delete", "DesCaseTypeDelete", "{\"area\":\"Patent\"}", 11),
                    ("Des Case Type Delete Ext", "DesCaseTypeDeleteExt", "{\"area\":\"Patent\"}", 12),
                    ("Des Case Type Ext", "DesCaseTypeExt", "{\"area\":\"Patent\"}", 13),
                    ("Des Case Type Fields", "DesCaseTypeFields", "{\"area\":\"Patent\"}", 20),
                    ("Des Case Type Fields Delete", "DesCaseTypeFieldsDelete", "{\"area\":\"Patent\"}", 21),
                    ("Des Case Type Fields Delete Ext", "DesCaseTypeFieldsDeleteExt", "{\"area\":\"Patent\"}", 22),
                    ("Des Case Type Fields Ext", "DesCaseTypeFieldsExt", "{\"area\":\"Patent\"}", 23),
                };

                // Trademark categories
                var tmkManage = new List<(string Title, string Controller, string RouteOptions, int SortOrder)>
                {
                    ("Country Due", "CountryDue", "{\"area\":\"Trademark\"}", 10),
                    ("Country Law", "CountryLaw", "{\"area\":\"Trademark\"}", 20),
                };
                var tmkAuxiliary = new List<(string Title, string Controller, string RouteOptions, int SortOrder)>
                {
                    ("Action Type", "ActionType", "{\"area\":\"Trademark\"}", 10),
                    ("Area", "Area", "{\"area\":\"Trademark\"}", 20),
                    ("Area Delete", "AreaDelete", "{\"area\":\"Trademark\"}", 21),
                    ("Area Country", "AreaCountry", "{\"area\":\"Trademark\"}", 22),
                    ("Area Country Delete", "AreaCountryDelete", "{\"area\":\"Trademark\"}", 23),
                    ("Case Type", "CaseType", "{\"area\":\"Trademark\"}", 30),
                    ("Country", "Country", "{\"area\":\"Trademark\"}", 40),
                    ("Indicator", "Indicator", "{\"area\":\"Trademark\"}", 50),
                    ("Standard Good", "StandardGood", "{\"area\":\"Trademark\"}", 60)
                };
                var tmkDesignated = new List<(string Title, string Controller, string RouteOptions, int SortOrder)>
                {
                    ("Des Case Type", "DesCaseType", "{\"area\":\"Trademark\"}", 10),
                    ("Des Case Type Delete", "DesCaseTypeDelete", "{\"area\":\"Trademark\"}", 11),
                    ("Des Case Type Delete Ext", "DesCaseTypeDeleteExt", "{\"area\":\"Trademark\"}", 12),
                    ("Des Case Type Ext", "DesCaseTypeExt", "{\"area\":\"Trademark\"}", 13),
                    ("Des Case Type Fields", "DesCaseTypeFields", "{\"area\":\"Trademark\"}", 20),
                    ("Des Case Type Fields Delete", "DesCaseTypeFieldsDelete", "{\"area\":\"Trademark\"}", 21),
                    ("Des Case Type Fields Delete Ext", "DesCaseTypeFieldsDeleteExt", "{\"area\":\"Trademark\"}", 22),
                    ("Des Case Type Fields Ext", "DesCaseTypeFieldsExt", "{\"area\":\"Trademark\"}", 23),
                };

                bool changed = false;

                changed |= await SeedCategory(db, menuItemRepo, menuPageRepo, allItems, allPages, patentTop.Id, "Patent", "Manage", 10, patManage, cancellationToken);
                changed |= await SeedCategory(db, menuItemRepo, menuPageRepo, allItems, allPages, patentTop.Id, "Patent", "Auxiliary", 20, patAuxiliary, cancellationToken);
                changed |= await SeedCategory(db, menuItemRepo, menuPageRepo, allItems, allPages, patentTop.Id, "Patent", "Designated", 30, patDesignated, cancellationToken);
                changed |= await SeedCategory(db, menuItemRepo, menuPageRepo, allItems, allPages, trademarkTop.Id, "Trademark", "Manage", 10, tmkManage, cancellationToken);
                changed |= await SeedCategory(db, menuItemRepo, menuPageRepo, allItems, allPages, trademarkTop.Id, "Trademark", "Auxiliary", 20, tmkAuxiliary, cancellationToken);
                changed |= await SeedCategory(db, menuItemRepo, menuPageRepo, allItems, allPages, trademarkTop.Id, "Trademark", "Designated", 30, tmkDesignated, cancellationToken);

                // Clean up: remove items from Auxiliary that now belong to Manage or Designated
                changed |= await CleanupMisplacedItems(db, menuItemRepo, allItems, allPages, patentTop.Id, "Patent",
                    patManage, patAuxiliary, patDesignated, cancellationToken);
                changed |= await CleanupMisplacedItems(db, menuItemRepo, allItems, allPages, trademarkTop.Id, "Trademark",
                    tmkManage, tmkAuxiliary, tmkDesignated, cancellationToken);

                // Seed "Releases" top-level tab (after Trademark, before Administration)
                changed |= await SeedReleasesTopLevel(db, menuItemRepo, menuPageRepo, allItems, allPages, trademarkTop, cancellationToken);

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

        private async Task<bool> SeedCategory(
            ICPiDbContext db,
            IRepository<CPiMenuItem> menuItemRepo,
            IRepository<CPiMenuPage> menuPageRepo,
            List<CPiMenuItem> allItems,
            List<CPiMenuPage> allPages,
            string parentTopId,
            string areaName,
            string categoryTitle,
            int categorySortOrder,
            List<(string Title, string Controller, string RouteOptions, int SortOrder)> items,
            CancellationToken cancellationToken)
        {
            bool changed = false;

            // Find or create category
            var category = allItems.FirstOrDefault(m => m.ParentId == parentTopId && m.Title == categoryTitle);
            if (category != null && string.IsNullOrEmpty(category.Policy))
            {
                category.Policy = "*";
                menuItemRepo.Update(category);
                await db.SaveChangesAsync();
                changed = true;
            }
            if (category == null)
            {
                category = new CPiMenuItem
                {
                    ParentId = parentTopId,
                    Title = categoryTitle,
                    SortOrder = categorySortOrder,
                    IsEnabled = true,
                    Policy = "*"
                };
                menuItemRepo.Add(category);
                await db.SaveChangesAsync();
                allItems.Add(category);
                _logger.LogInformation("AuxiliaryMenuSeeder: Created '{Category}' category under {Area}.", categoryTitle, areaName);
                changed = true;
            }
            var auxCategory = category;

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

        private async Task<bool> SeedReleasesTopLevel(
            ICPiDbContext db,
            IRepository<CPiMenuItem> menuItemRepo,
            IRepository<CPiMenuPage> menuPageRepo,
            List<CPiMenuItem> allItems,
            List<CPiMenuPage> allPages,
            CPiMenuItem trademarkTop,
            CancellationToken cancellationToken)
        {
            bool changed = false;

            // Find or create the "Releases" top-level menu item
            var releasesTop = allItems.FirstOrDefault(m => m.ParentId == "" && m.Title == "Releases");
            if (releasesTop == null)
            {
                releasesTop = new CPiMenuItem
                {
                    Id = "Releases",
                    ParentId = "",
                    Title = "Releases",
                    SortOrder = trademarkTop.SortOrder + 1,
                    IsEnabled = true,
                    Policy = "*"
                };
                menuItemRepo.Add(releasesTop);
                await db.SaveChangesAsync();
                allItems.Add(releasesTop);
                _logger.LogInformation("AuxiliaryMenuSeeder: Created 'Releases' top-level menu item.");
                changed = true;
            }

            // Find or create "Manage" subcategory
            var manageCategory = allItems.FirstOrDefault(m => m.ParentId == releasesTop.Id && m.Title == "Manage");
            if (manageCategory == null)
            {
                manageCategory = new CPiMenuItem
                {
                    ParentId = releasesTop.Id,
                    Title = "Manage",
                    SortOrder = 10,
                    IsEnabled = true,
                    Policy = "*"
                };
                menuItemRepo.Add(manageCategory);
                await db.SaveChangesAsync();
                allItems.Add(manageCategory);
                _logger.LogInformation("AuxiliaryMenuSeeder: Created 'Manage' category under Releases.");
                changed = true;
            }

            // Find or create the "Release" page
            var routeOptions = "{\"area\":\"Releases\"}";
            var releasePage = allPages.FirstOrDefault(p => p.Controller == "Release" && p.RouteOptions == routeOptions);
            if (releasePage == null)
            {
                releasePage = new CPiMenuPage
                {
                    Name = "Releases",
                    Controller = "Release",
                    Action = "Index",
                    RouteOptions = routeOptions,
                    Policy = "*"
                };
                menuPageRepo.Add(releasePage);
                await db.SaveChangesAsync();
                allPages.Add(releasePage);
                _logger.LogInformation("AuxiliaryMenuSeeder: Created 'Releases' page.");
                changed = true;
            }

            // Find or create the "Release" leaf menu item
            var releaseLeaf = allItems.FirstOrDefault(m => m.ParentId == manageCategory.Id && m.PageId == releasePage.Id);
            if (releaseLeaf == null)
            {
                releaseLeaf = new CPiMenuItem
                {
                    ParentId = manageCategory.Id,
                    Title = "Releases",
                    PageId = releasePage.Id,
                    SortOrder = 10,
                    IsEnabled = true,
                    Policy = "*"
                };
                menuItemRepo.Add(releaseLeaf);
                await db.SaveChangesAsync();
                allItems.Add(releaseLeaf);
                _logger.LogInformation("AuxiliaryMenuSeeder: Created 'Releases' menu item under Releases/Manage.");
                changed = true;
            }

            // Find or create "Auxiliary" subcategory under Releases
            var auxCategory = allItems.FirstOrDefault(m => m.ParentId == releasesTop.Id && m.Title == "Auxiliary");
            if (auxCategory == null)
            {
                auxCategory = new CPiMenuItem
                {
                    ParentId = releasesTop.Id,
                    Title = "Auxiliary",
                    SortOrder = 20,
                    IsEnabled = true,
                    Policy = "*"
                };
                menuItemRepo.Add(auxCategory);
                await db.SaveChangesAsync();
                allItems.Add(auxCategory);
                _logger.LogInformation("AuxiliaryMenuSeeder: Created 'Auxiliary' category under Releases.");
                changed = true;
            }

            // Find or create the "Systems" page and menu item
            var systemRouteOptions = "{\"area\":\"Shared\"}";
            var systemPage = allPages.FirstOrDefault(p => p.Controller == "System" && p.RouteOptions == systemRouteOptions);
            if (systemPage == null)
            {
                systemPage = new CPiMenuPage
                {
                    Name = "Systems",
                    Controller = "System",
                    Action = "Index",
                    RouteOptions = systemRouteOptions,
                    Policy = "*"
                };
                menuPageRepo.Add(systemPage);
                await db.SaveChangesAsync();
                allPages.Add(systemPage);
                _logger.LogInformation("AuxiliaryMenuSeeder: Created 'Systems' page.");
                changed = true;
            }

            var systemLeaf = allItems.FirstOrDefault(m => m.ParentId == auxCategory.Id && m.PageId == systemPage.Id);
            if (systemLeaf == null)
            {
                systemLeaf = new CPiMenuItem
                {
                    ParentId = auxCategory.Id,
                    Title = "Systems",
                    PageId = systemPage.Id,
                    SortOrder = 10,
                    IsEnabled = true,
                    Policy = "*"
                };
                menuItemRepo.Add(systemLeaf);
                await db.SaveChangesAsync();
                allItems.Add(systemLeaf);
                _logger.LogInformation("AuxiliaryMenuSeeder: Created 'Systems' menu item under Releases/Auxiliary.");
                changed = true;
            }

            return changed;
        }

        private async Task<bool> CleanupMisplacedItems(
            ICPiDbContext db,
            IRepository<CPiMenuItem> menuItemRepo,
            List<CPiMenuItem> allItems,
            List<CPiMenuPage> allPages,
            string parentTopId,
            string areaName,
            List<(string Title, string Controller, string RouteOptions, int SortOrder)> manageItems,
            List<(string Title, string Controller, string RouteOptions, int SortOrder)> auxiliaryItems,
            List<(string Title, string Controller, string RouteOptions, int SortOrder)> designatedItems,
            CancellationToken cancellationToken)
        {
            bool changed = false;
            var allCategories = new[] { ("Manage", manageItems), ("Auxiliary", auxiliaryItems), ("Designated", designatedItems) };

            foreach (var (catTitle, catItems) in allCategories)
            {
                var category = allItems.FirstOrDefault(m => m.ParentId == parentTopId && m.Title == catTitle);
                if (category == null) continue;

                var validControllers = new HashSet<string>(catItems.Select(i => i.Controller), StringComparer.OrdinalIgnoreCase);
                var leafItems = allItems.Where(m => m.ParentId == category.Id && m.PageId.HasValue).ToList();

                foreach (var leaf in leafItems)
                {
                    var page = allPages.FirstOrDefault(p => p.Id == leaf.PageId);
                    if (page == null) continue;

                    if (!validControllers.Contains(page.Controller ?? ""))
                    {
                        menuItemRepo.Delete(leaf);
                        await db.SaveChangesAsync();
                        allItems.Remove(leaf);
                        _logger.LogInformation("AuxiliaryMenuSeeder: Removed '{Title}' from {Area}/{Category}.", leaf.Title, areaName, catTitle);
                        changed = true;
                    }
                }
            }
            return changed;
        }
    }
}
