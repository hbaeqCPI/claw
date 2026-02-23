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
    public interface IAgentViewModelService
    {
        Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<Agent> agents);
        IQueryable<Agent> AddCriteria(IQueryable<Agent> agents, List<QueryFilterViewModel> mainSearchFilters);
        Task<AgentDetailViewModel> CreateViewModelForDetailScreen(int id);
        Task<List<AgentContactViewModel>> GetAgentContacts(int agentId);
        AgentContact MapToDomainModel(AgentContactViewModel agentContactVM);
        Task<int?> GetAgentId(string agentCode);
        List<LetterOptionViewModel> GetLetterOptions();
        List<SendAsOptionViewModel> GetSendAsOptions();

    }
}

 