using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.DTOs;
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
    public class ClientViewModelService : IClientViewModelService
    {
        private readonly IClientService _clientService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public ClientViewModelService(IClientService clientService, IMapper mapper, IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _clientService = clientService;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        public IQueryable<Client> AddCriteria(IQueryable<Client> clients, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {
                var patAtty = mainSearchFilters.FirstOrDefault(f => f.Property == "PatAttorney");
                if (patAtty != null)
                {
                    clients = clients.Where(c => EF.Functions.Like(c.PatDefaultAtty1.AttorneyCode, patAtty.Value) || EF.Functions.Like(c.PatDefaultAtty2.AttorneyCode, patAtty.Value) || EF.Functions.Like(c.PatDefaultAtty3.AttorneyCode, patAtty.Value) || EF.Functions.Like(c.PatDefaultAtty4.AttorneyCode, patAtty.Value) || EF.Functions.Like(c.PatDefaultAtty5.AttorneyCode, patAtty.Value));
                    mainSearchFilters.Remove(patAtty);
                }

                var patAttyName = mainSearchFilters.FirstOrDefault(f => f.Property == "PatAttorneyName");
                if (patAttyName != null)
                {
                    clients = clients.Where(c => EF.Functions.Like(c.PatDefaultAtty1.AttorneyName, patAttyName.Value) || EF.Functions.Like(c.PatDefaultAtty2.AttorneyName, patAttyName.Value) || EF.Functions.Like(c.PatDefaultAtty3.AttorneyName, patAttyName.Value) || EF.Functions.Like(c.PatDefaultAtty4.AttorneyName, patAttyName.Value) || EF.Functions.Like(c.PatDefaultAtty5.AttorneyName, patAttyName.Value));
                    mainSearchFilters.Remove(patAttyName);
                }

                var tmkAtty = mainSearchFilters.FirstOrDefault(f => f.Property == "TmkAttorney");
                if (tmkAtty != null)
                {
                    clients = clients.Where(c => EF.Functions.Like(c.TmkDefaultAtty1.AttorneyCode, tmkAtty.Value) || EF.Functions.Like(c.TmkDefaultAtty2.AttorneyCode, tmkAtty.Value) || EF.Functions.Like(c.TmkDefaultAtty3.AttorneyCode, tmkAtty.Value) || EF.Functions.Like(c.TmkDefaultAtty4.AttorneyCode, tmkAtty.Value) || EF.Functions.Like(c.TmkDefaultAtty5.AttorneyCode, tmkAtty.Value));
                    mainSearchFilters.Remove(tmkAtty);
                }

                var tmkAttyName = mainSearchFilters.FirstOrDefault(f => f.Property == "TmkAttorneyName");
                if (tmkAttyName != null)
                {
                    clients = clients.Where(c => EF.Functions.Like(c.TmkDefaultAtty1.AttorneyName, tmkAttyName.Value) || EF.Functions.Like(c.TmkDefaultAtty2.AttorneyName, tmkAttyName.Value) || EF.Functions.Like(c.TmkDefaultAtty3.AttorneyName, tmkAttyName.Value) || EF.Functions.Like(c.TmkDefaultAtty4.AttorneyName, tmkAttyName.Value) || EF.Functions.Like(c.TmkDefaultAtty5.AttorneyName, tmkAttyName.Value));
                    mainSearchFilters.Remove(tmkAttyName);
                }

                var contact = mainSearchFilters.FirstOrDefault(f => f.Property == "Contact");
                if (contact != null)
                {
                    clients = clients.Where(w => w.ClientContacts.Any(a => EF.Functions.Like(a.Contact.Contact, contact.Value)));
                    mainSearchFilters.Remove(contact);
                }

                var contactName = mainSearchFilters.FirstOrDefault(f => f.Property == "ContactName");
                if (contactName != null)
                {
                    clients = clients.Where(w => w.ClientContacts.Any(a => EF.Functions.Like(a.Contact.ContactName, contactName.Value)));
                    mainSearchFilters.Remove(contactName);
                }

                if (mainSearchFilters.Any())
                    clients = QueryHelper.BuildCriteria<Client>(clients, mainSearchFilters);
            }
            return clients;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<Client> clients)
        {
            var model = clients.ProjectTo<ClientSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(client => client.ClientCode);

            var ids = await model.Select(c => c.ClientID).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<ClientDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var client = await _clientService.QueryableList.Where(c => c.ClientID == id).ProjectTo<ClientDetailViewModel>().FirstOrDefaultAsync();
            return client;
        }

        public async Task<List<ClientContactViewModel>> GetClientContacts(int clientId)
        {
            var vm = await _clientService.ChildService.QueryableList.Where(c => c.ClientID == clientId).ProjectTo<ClientContactViewModel>().ToListAsync();
            var sendAsOptions = SendAsOptionViewModel.BuildList(_sharedLocalizer);
            var letterOptions = LetterOptionViewModel.BuildList(_sharedLocalizer);

            vm.ForEach(cc =>
            {
                cc.LetterSendAsDescription = sendAsOptions.Where(o => o.LetterSendAs.ToLower() == cc.LetterSendAs.ToLower()).Select(o => o.Description).FirstOrDefault();
                cc.GenAllLettersDescription = letterOptions.Where(o => o.GenAllLetters == cc.GenAllLetters).Select(o => o.Description).FirstOrDefault();
            });
            return vm;
        }

        public ClientContact MapToDomainModel(ClientContactViewModel clientContactVM)
        {
            var clientContact = _mapper.Map<ClientContact>(clientContactVM);
            return clientContact;
        }

        public async Task<int?> GetClientId(string clientCode)
        {
            var client = await _clientService.QueryableList.Where(c => c.ClientCode == clientCode).FirstOrDefaultAsync();
            return client?.ClientID;
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
