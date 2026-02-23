using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;

namespace R10.Web.Interfaces
{
    public interface IClientViewModelService
    {
        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<Client> clients);
        IQueryable<Client> AddCriteria(IQueryable<Client> clients, List<QueryFilterViewModel> mainSearchFilters);
        Task<ClientDetailViewModel> CreateViewModelForDetailScreen(int id);
        Task<List<ClientContactViewModel>> GetClientContacts(int clientId);
        ClientContact MapToDomainModel(ClientContactViewModel agentContactVM);
        Task<int?> GetClientId(string clientCode);
        //moved to controller to simplify and reuse authorization and url helper
        //Task<List<DetailPageAction>> GetMoreScreenOptions(string baseUrlPath, string clientCode);
        List<LetterOptionViewModel> GetLetterOptions();
        List<SendAsOptionViewModel> GetSendAsOptions();
    }
}

 