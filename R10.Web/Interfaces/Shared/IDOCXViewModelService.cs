using Kendo.Mvc.UI;
using R10.Core.Entities;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IDOCXViewModelService
    {
        #region DOCX Setup Screen
        IQueryable<DOCXMain> AddCriteria(IQueryable<DOCXMain> DOCXes, List<QueryFilterViewModel> mainSearchFilters);

        Task<CPiDataSourceResult> CreateViewModelForSearchGrid(DataSourceRequest request, IQueryable<DOCXMain> DOCXesMain);

        Task<DOCXMainDetailViewModel> CreateViewModelForDetailScreen(int id);
        #endregion

        #region DOCX Popup Screen
        Task<CPiDataSourceResult> CreateViewModelForDOCXGrid(DataSourceRequest request, string systemType, string screenCode, string? DOCXName);
        
        #endregion

    }
}
