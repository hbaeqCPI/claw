using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class AgentViewModelService : IAgentViewModelService
    {
        private readonly IAgentService _agentService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public AgentViewModelService(IAgentService agentService, IMapper mapper, IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _agentService = agentService;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        public IQueryable<Agent> AddCriteria(IQueryable<Agent> agents, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {
                var contact = mainSearchFilters.FirstOrDefault(f => f.Property == "Contact");
                if (contact != null)
                {
                    agents = agents.Where(w => w.AgentContacts.Any(a => EF.Functions.Like(a.Contact.Contact, contact.Value)));
                    mainSearchFilters.Remove(contact);
                }

                var contactName = mainSearchFilters.FirstOrDefault(f => f.Property == "ContactName");
                if (contactName != null)
                {
                    agents = agents.Where(w => w.AgentContacts.Any(a => EF.Functions.Like(a.Contact.ContactName, contactName.Value)));
                    mainSearchFilters.Remove(contactName);
                }

                if (mainSearchFilters.Any())
                    agents = QueryHelper.BuildCriteria(agents, mainSearchFilters);
            }
            return agents;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<Agent> agents)
        {
            var model = agents.ProjectTo<AgentSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(agent => agent.AgentCode);

            var ids = await model.Select(c => c.AgentID).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<AgentDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var agent =  await _agentService.QueryableList.Where(a => a.AgentID == id).ProjectTo<AgentDetailViewModel>().FirstOrDefaultAsync();
            return agent;
        }

       
        public async Task<List<AgentContactViewModel>> GetAgentContacts(int agentId)
        {
            var vm = await _agentService.ChildService.QueryableList.Where(c => c.AgentID == agentId).ProjectTo<AgentContactViewModel>().ToListAsync();
            var sendAsOptions = SendAsOptionViewModel.BuildList(_sharedLocalizer);
            var letterOptions = LetterOptionViewModel.BuildList(_sharedLocalizer);

            vm.ForEach(cc =>
            {
                cc.LetterSendAsDescription = sendAsOptions.Where(o => o.LetterSendAs.ToLower() == cc.LetterSendAs.ToLower()).Select(o => o.Description).FirstOrDefault();
                cc.GenAllLettersDescription = letterOptions.Where(o => o.GenAllLetters == cc.GenAllLetters).Select(o => o.Description).FirstOrDefault();
            });
            return vm;
        }


        public AgentContact MapToDomainModel(AgentContactViewModel agentContactVM)
        {
            var agentContact = _mapper.Map<AgentContact>(agentContactVM);
            return agentContact;
        }

        public async Task<int?> GetAgentId(string agentCode)
        {
            var agent = await _agentService.QueryableList.Where(a => a.AgentCode == agentCode).FirstOrDefaultAsync();
            return agent?.AgentID;
        }

        public List<LetterOptionViewModel> GetLetterOptions()
        {
            return LetterOptionViewModel.BuildList(_sharedLocalizer);
        }

        public List<SendAsOptionViewModel> GetSendAsOptions()
        {
            return SendAsOptionViewModel.BuildList(_sharedLocalizer);
        }





    }
}
