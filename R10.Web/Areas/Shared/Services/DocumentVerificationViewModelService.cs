using R10.Web.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Queries.Shared;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Security.Claims;
using R10.Core.Helpers;

namespace R10.Web.Services
{
    public class DocumentVerificationViewModelService : IDocumentVerificationViewModelService
    {

        private readonly IDocumentVerificationRepository _docVerificationRepository;
        private readonly IMapper _mapper;
        protected readonly ClaimsPrincipal _user;

        public DocumentVerificationViewModelService(
                                IDocumentVerificationRepository docVerificationRepository,
                                IMapper mapper,
                                ClaimsPrincipal user)
        {
            _docVerificationRepository = docVerificationRepository;
            _mapper = mapper;
            _user = user;
        }

        public async Task<List<DocumentVerificationNewDTO>> GetDocVerificationNewDocs(DocumentVerificationSearchCriteriaViewModel viewModel)
        {
            var criteria = _mapper.Map<DocumentVerificationSearchCriteriaDTO>(viewModel);
            criteria.SystemTypes = GetSystemTypes(viewModel);
            criteria.UserName = _user.GetEmail();
            criteria.TargetData = "newList";
            var list = await _docVerificationRepository.GetDocVerificationNewDocs(criteria);
            return list;
        }

        public async Task<List<DocumentVerificationDTO>> GetDocVerificationDocuments(DocumentVerificationSearchCriteriaViewModel viewModel)
        {
            var criteria = _mapper.Map<DocumentVerificationSearchCriteriaDTO>(viewModel);
            criteria.SystemTypes = GetSystemTypes(viewModel);
            criteria.UserName = _user.GetEmail();
            var list = await _docVerificationRepository.GetDocVerificationDocuments(criteria);
            return list;
        }

        public async Task<List<DocumentVerificationActionDTO>> GetDocVerificationActions(DocumentVerificationSearchCriteriaViewModel viewModel)
        {
            var criteria = _mapper.Map<DocumentVerificationSearchCriteriaDTO>(viewModel);
            criteria.SystemTypes = GetSystemTypes(viewModel);
            criteria.UserName = _user.GetEmail();
            criteria.TargetData = "actList";
            var list = await _docVerificationRepository.GetDocVerificationActions(criteria);
            return list;
        }

        public async Task<List<DocumentVerificationNewDTO>> GetDocVerificationNewDocById(string ids)
        {
            var list = await _docVerificationRepository.GetDocVerificationNewDocExport(ids);
            return list;
        }
        public async Task<List<DocumentVerificationDTO>> GetDocVerificationDocById(string ids)
        {
            var list = await _docVerificationRepository.GetDocVerificationDocExport(ids);
            return list;
        }
        public async Task<List<DocumentVerificationActionDTO>> GetDocVerificationActionDocById(string ids)
        {
            var list = await _docVerificationRepository.GetDocVerificationActionDocExport(ids);
            return list;
        }
        public async Task<List<DocumentVerificationCommunicationDTO>> GetDocVerificationCommunicationsDocById(string ids)
        {
            var list = await _docVerificationRepository.GetDocVerificationCommunicationsDocExport(ids);
            return list;
        }

        public async Task<List<DocumentVerificationCommunicationDTO>> GetDocVerificationCommunications(DocumentVerificationSearchCriteriaViewModel viewModel)
        {
            var criteria = _mapper.Map<DocumentVerificationSearchCriteriaDTO>(viewModel);
            criteria.SystemTypes = GetSystemTypes(viewModel);
            criteria.UserName = _user.GetEmail();
            criteria.TargetData = "commList";
            var list = await _docVerificationRepository.GetDocVerificationCommunications(criteria);
            return list;
        }

        private string GetSystemTypes(DocumentVerificationSearchCriteriaViewModel criteria)
        {
            var systemTypes = "|";
            if (criteria.Patent == "P")
                systemTypes = systemTypes + criteria.Patent + "|";

            if (criteria.Trademark == "T")
                systemTypes = systemTypes + criteria.Trademark + "|";

            if (criteria.GeneralMatter == "G")
                systemTypes = systemTypes + criteria.GeneralMatter + "|";

            if (criteria.ShowOrphanage)
                systemTypes = systemTypes + "O|";

            return systemTypes;
        }
    }
}
