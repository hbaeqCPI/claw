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
    public interface ILetterViewModelService
    {
        #region Letter Setup Screen
        IQueryable<LetterMain> AddCriteria(IQueryable<LetterMain> letters, List<QueryFilterViewModel> mainSearchFilters);

        Task<CPiDataSourceResult> CreateViewModelForSearchGrid(DataSourceRequest request, IQueryable<LetterMain> lettersMain);

        Task<LetterMainDetailViewModel> CreateViewModelForDetailScreen(int id);
        #endregion

        #region Letter Popup Screen
        Task<CPiDataSourceResult> CreateViewModelForLetterGrid(DataSourceRequest request, string systemType, string screenCode, string? letterName, int? letCatId, int? letSubCatId, List<string>? tags);
        
        #endregion

    }
}
