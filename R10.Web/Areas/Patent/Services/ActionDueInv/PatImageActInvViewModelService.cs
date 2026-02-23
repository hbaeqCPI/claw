using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Helpers;
using System.Security.Claims;
using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Areas.Patent.Services
{
    public class PatImageActInvViewModelService : IPatImageActInvViewModelService
    {
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IDocumentService _docService;
        private readonly ClaimsPrincipal _user;
        private readonly IApplicationDbContext _repository;

        public PatImageActInvViewModelService(
            ISystemSettings<PatSetting> settings,
            IDocumentService docService,
            ClaimsPrincipal user,
            IApplicationDbContext repository)
        {
            _settings = settings;
            _docService = docService;
            _user = user;
            _repository = repository;
        }

        public async Task<List<DocDocumentListViewModel>> CreateViewModelForList(int parentId)
        {
            var settings = await _settings.GetSetting();
            //var docs = await _docService.DocDocuments.Where(d => _docService.DocFolders.Where(f => f.SystemType == SystemTypeCode.Patent && f.DataKey == "ActId" && f.DataKeyValue == parentId)
            //                    .Any(f => f.FolderId == d.FolderId))
            //                    .ProjectTo<DocDocumentListViewModel>()
            //                    .ToListAsync();
            var docs = new List<DocDocumentListViewModel>();

            if (settings.IsDocumentVerificationOn)
            {
                docs = await _docService.DocDocuments.Where(d => _docService.DocFolders
                                        .Where(f => (f.SystemType == SystemTypeCode.Patent &&  f.DataKey == "ActInvId" && f.DataKeyValue == parentId)
                                                || (f.SystemType == SystemTypeCode.Patent &&  f.DataKey == "InvId" && _repository.PatActionDueInvs.Any(a => a.InvId == f.DataKeyValue && a.ActId == parentId))
                                            )
                                .Any(f => f.FolderId == d.FolderId))
                                .ProjectTo<DocDocumentListViewModel>()
                                .OrderByDescending(o => o.DateCreated)
                                .ToListAsync();

                var docIds = docs.Select(d => d.DocId).Distinct().ToList();
                var docVerifications = await _docService.DocVerifications.Where(vf => vf.ActId == parentId && docIds.Contains(vf.DocId ?? 0)).ToListAsync();

                foreach (var doc in docs)
                {
                    if (docVerifications.Any(d => d.DocId == doc.DocId))
                        doc.IsDocVerificationLinked = true;
                }
            }
            else
            {
                docs = await _docService.DocDocuments.Where(d => _docService.DocFolders
                                            .Where(f => f.SystemType == SystemTypeCode.Patent && f.DataKey == "ActInvId" && f.DataKeyValue == parentId && f.ScreenCode == ScreenCode.ActionInv)
                                            .Any(f => f.FolderId == d.FolderId)
                                            )
                                        .ProjectTo<DocDocumentListViewModel>()
                                        .OrderByDescending(o => o.DateCreated)
                                        .ToListAsync();
            }

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


    }
}
