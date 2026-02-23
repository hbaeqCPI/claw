using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Identity;
using R10.Core.Queries.Shared;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Interfaces
{
    public interface IOuickEmailSetupViewModelService
    {

        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<QEMain> list);
        IQueryable<QEMain> AddCriteria(IQueryable<QEMain> templates, List<QueryFilterViewModel> mainSearchFilters);

        IQueryable<QEDetailView> GetQuickEmails(string systemType);
        Task<QuickEmailSetupDetailViewModel> GetQuickEmailSetupById(int id);
        Task<QEDetailView> GetQuickEmailByName(string systemType, string templateName);
        IQueryable<QERecipient> GetRecipients(int id);
        Task<QEDataSource> GetDataSource(string systemType, string dataSourceName);

    }
}

 