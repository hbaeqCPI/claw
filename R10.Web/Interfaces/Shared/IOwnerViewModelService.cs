using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Core.Entities;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Interfaces
{
    public interface IOwnerViewModelService
    {
        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<Owner> owners);
        IQueryable<Owner> AddCriteria(IQueryable<Owner> owners, List<QueryFilterViewModel> mainSearchFilters);
        Task<OwnerDetailViewModel> CreateViewModelForDetailScreen(int id);
        Task<List<OwnerContactViewModel>> GetOwnerContacts(int ownerId);
        OwnerContact MapToDomainModel(OwnerContactViewModel ownerContactVM);
        Task<int?> GetOwnerId(string ownerCode);

        List<LetterOptionViewModel> GetLetterOptions();
        List<SendAsOptionViewModel> GetSendAsOptions();
        
        //moved to controller to simplify and reuse authorization and url helper
        //Task<List<DetailPageAction>> GetMoreScreenOptions(string baseUrlPath, string ownerCode);

    }
}

 