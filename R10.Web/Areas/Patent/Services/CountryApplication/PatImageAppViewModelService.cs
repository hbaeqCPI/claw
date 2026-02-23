using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Core.Services;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Helpers;
using System.Security.Claims;
using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Areas.Patent.Services
{
    public class PatImageAppViewModelService : IPatImageAppViewModelService
    {
        private readonly IDocumentService _docService;
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly ClaimsPrincipal _user;

        public PatImageAppViewModelService(
            IDocumentService docService,
            IApplicationDbContext repository,
            ISystemSettings<PatSetting> settings,
            ClaimsPrincipal user)
        {
            _docService = docService;
            _repository = repository;
            _settings = settings;
            _user = user;
        }

        public async Task<List<DocDocumentListViewModel>> CreateViewModelForList(int parentId)
        {
            var settings = await _settings.GetSetting();
            var docs = await _docService.DocDocuments.Where(d => _docService.DocFolders.Where(f => f.SystemType == SystemTypeCode.Patent && ((f.DataKey == "AppId" && f.DataKeyValue == parentId) ||
                                                                  (f.DataKey == "ActId" && _repository.PatActionDues.Any(a => a.AppId == parentId && a.ActId == f.DataKeyValue)) ||
                                                                  (f.DataKey == "CostTrackId" && _repository.PatCostTracks.Any(a => a.AppId == parentId && a.CostTrackId == f.DataKeyValue)) ||
                                                                  (f.DataKey == "InvId" && _repository.Inventions.Any(i => i.InvId == f.DataKeyValue && i.CountryApplications.Any(ca => ca.AppId == parentId))))
                                                             )
                                .Any(f => f.FolderId == d.FolderId))
                                .ProjectTo<DocDocumentListViewModel>()
                                .ToListAsync();

            var canViewPublicOnly = settings.IsRestrictPrivateDocAccessOn && await _docService.IsUserRestrictedFromPrivateDocuments();
            if (canViewPublicOnly)
            {
                docs = docs.Where(d => (d.FolderIsPublic || d.FolderCreatedBy == _user.GetUserName()) && (!d.IsPrivate || d.CreatedBy == _user.GetUserName())).ToList();
            }

            //docs.ForEach(d => {
            //    if (d.ThumbFileName != null && !d.ThumbFileName.StartsWith("logo"))
            //        d.ThumbFileName = ImageHelper.GetThumbnailIcon(d.DocFileName, d.ThumbFileName) ?? "";
            //});
            return docs;
        }

        public async Task<List<DocDocumentListViewModel>> CreateViewModelForDownload(int parentId, string selection)
        {
            var keys = selection.Split(',');
            var selectionList = keys.Select(k => new DocDocumentListViewModel { DocId = Convert.ToInt32(k) });

            var docs = await _docService.DocDocuments.Where(d => _docService.DocFolders.Where(f => f.SystemType == SystemTypeCode.Patent && ((f.DataKey == "AppId" && f.DataKeyValue == parentId) ||
                                                                  (f.DataKey == "ActId" && _repository.PatActionDues.Any(a => a.AppId == parentId && a.ActId == f.DataKeyValue)) ||
                                                                  (f.DataKey == "CostTrackId" && _repository.PatCostTracks.Any(a => a.AppId == parentId && a.CostTrackId == f.DataKeyValue)) ||
                                                                  (f.DataKey == "InvId" && _repository.Inventions.Any(i => i.InvId == f.DataKeyValue && i.CountryApplications.Any(ca => ca.AppId == parentId))))
                                                             )
                                .Any(f => f.FolderId == d.FolderId)).Select(d =>

                                  new DocDocumentListViewModel
                                  {
                                      DocId = d.DocId,
                                      UserFileName = d.DocFile.UserFileName,
                                      DocFileName = d.DocFile.DocFileName,
                                      FolderId = d.FolderId,
                                  }
                             ).ToListAsync();
            docs = docs.Where(d => selectionList.Any(s => s.DocId == d.DocId) && !string.IsNullOrEmpty(d.DocFileName)).ToList();
            return docs;
        }


    }
}
