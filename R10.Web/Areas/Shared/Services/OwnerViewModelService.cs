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
    public class OwnerViewModelService : IOwnerViewModelService
    {
        private readonly IOwnerService _ownerService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public OwnerViewModelService(IOwnerService ownerService, IMapper mapper, IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _ownerService = ownerService;
            _mapper = mapper;
            _sharedLocalizer = sharedLocalizer;
        }

        public IQueryable<Owner> AddCriteria(IQueryable<Owner> owners, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.Count > 0)
            {
                var contact = mainSearchFilters.FirstOrDefault(f => f.Property == "Contact");
                if (contact != null)
                {
                    owners = owners.Where(w => w.OwnerContacts.Any(a => EF.Functions.Like(a.Contact.Contact, contact.Value)));
                    mainSearchFilters.Remove(contact);
                }

                var contactName = mainSearchFilters.FirstOrDefault(f => f.Property == "ContactName");
                if (contactName != null)
                {
                    owners = owners.Where(w => w.OwnerContacts.Any(a => EF.Functions.Like(a.Contact.ContactName, contactName.Value)));
                    mainSearchFilters.Remove(contactName);
                }

                if (mainSearchFilters.Any())
                    owners = QueryHelper.BuildCriteria(owners, mainSearchFilters);
            }
            return owners;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<Owner> owners)
        {
            var model = owners.ProjectTo<OwnerSearchResultViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(owner => owner.OwnerCode);

            var ids = await model.Select(c => c.OwnerID).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<OwnerDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var owner = await _ownerService.QueryableList.Where(c => c.OwnerID == id).ProjectTo<OwnerDetailViewModel>().FirstOrDefaultAsync();
            return owner;
        }

       
        public async Task<List<OwnerContactViewModel>> GetOwnerContacts(int ownerId)
        {
            var vm = await _ownerService.ChildService.QueryableList.Where(c => c.OwnerID == ownerId).ProjectTo<OwnerContactViewModel>().ToListAsync();
            var sendAsOptions = SendAsOptionViewModel.BuildList(_sharedLocalizer);
            var letterOptions = LetterOptionViewModel.BuildList(_sharedLocalizer);

            vm.ForEach(cc =>
            {
                cc.LetterSendAsDescription = sendAsOptions.Where(o => o.LetterSendAs.ToLower() == cc.LetterSendAs.ToLower()).Select(o => o.Description).FirstOrDefault();
                cc.GenAllLettersDescription = letterOptions.Where(o => o.GenAllLetters == cc.GenAllLetters).Select(o => o.Description).FirstOrDefault();
            });
            return vm;
        }

        public OwnerContact MapToDomainModel(OwnerContactViewModel ownerContactVM)
        {
            var ownerContact = _mapper.Map<OwnerContact>(ownerContactVM);
            return ownerContact;
        }

        public async Task<int?> GetOwnerId(string ownerCode)
        {
            var owner = await _ownerService.QueryableList.Where(o => o.OwnerCode == ownerCode).FirstOrDefaultAsync();
            return owner?.OwnerID;
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
