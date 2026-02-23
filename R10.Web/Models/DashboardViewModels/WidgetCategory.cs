using R10.Core.Entities;

namespace R10.Web.Models.DashboardViewModels
{
    public class WidgetCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public int SortOrder { get; set; }

        public static int DefaultCategoryId => 1;
        public static int CustomWidgetsCategoryId => 7;

        public static List<WidgetCategory> Categories =>
            new List<WidgetCategory>()
            {
                new WidgetCategory() { Id = 1, Name = "RecentActivity", DisplayName = "Recent Activity", Icon = "fal fa-calendar", SortOrder = 1 },
                new WidgetCategory() { Id = 2, Name = "Portfolio", DisplayName = "Portfolio", Icon = "fal fa-briefcase", SortOrder = 2 },
                new WidgetCategory() { Id = 3, Name = "Trends", DisplayName = "Trends", Icon = "fal fa-analytics", SortOrder = 3 },
                new WidgetCategory() { Id = 4, Name = "CostsProjections", DisplayName = "Costs & Projections", Icon = "fal fa-coins", SortOrder = 4 },
                new WidgetCategory() { Id = 5, Name = "DataWatch", DisplayName = "Data Watch", Icon = "fal fa-glasses", SortOrder = 5 },
                new WidgetCategory() { Id = 6, Name = "Miscellaneous", DisplayName = "Miscellaneous", Icon = "fal fa-clipboard", SortOrder = 6 },
                new WidgetCategory() { Id = 7, Name = "CustomWidgets", DisplayName = "My Widgets", Icon = "fal fa-user", SortOrder = 7 }
            };
    }

    public class WidgetSystem
    {        
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int SortOrder { get; set; }

        public static List<WidgetSystem> Systems =>
            new List<WidgetSystem>()
            {
                new WidgetSystem() { Name = SystemType.Patent, DisplayName = "Patent", SortOrder = 1 },
                new WidgetSystem() { Name = "inventorawards", DisplayName = "Inventor Awards", SortOrder = 2 },
                new WidgetSystem() { Name = SystemType.PatClearance, DisplayName = "Patent Clearance", SortOrder = 3 },
                new WidgetSystem() { Name = SystemType.Trademark, DisplayName = "Trademark", SortOrder = 4 },
                new WidgetSystem() { Name = SystemType.SearchRequest, DisplayName = "Search Request",  SortOrder = 5 },
                new WidgetSystem() { Name = SystemType.GeneralMatter, DisplayName = "General Matter", SortOrder = 6 },
                new WidgetSystem() { Name = SystemType.AMS, DisplayName = "AMS", SortOrder = 7 },
                new WidgetSystem() { Name = SystemType.DMS, DisplayName = "Invention Disclosure", SortOrder = 8 },
                new WidgetSystem() { Name = SystemType.Shared, DisplayName = "Shared", SortOrder = 9 },
                new WidgetSystem() { Name = "product", DisplayName = "Product", SortOrder = 10 }
            };
    }
}
