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
using R10.Web.Areas.Shared.ViewModels;
using System.Security.Claims;

namespace R10.Web.Areas.Patent.Services
{
    public class PatImageInvViewModelService : IPatImageInvViewModelService
    {        
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IDocumentService _docService;
        private readonly ClaimsPrincipal _user;

        public PatImageInvViewModelService(
            ISystemSettings<PatSetting> settings,
            IDocumentService docService,
            ClaimsPrincipal user)
        {
            _settings = settings;
            _docService = docService;
            _user = user;
        }

        public async Task<List<DocDocumentListViewModel>> CreateViewModelForList(int parentId)
        {
            var settings = await _settings.GetSetting();
            var docs = await _docService.DocDocuments.Where(d => _docService.DocFolders.Where(f => f.DataKey == "InvId" && f.DataKeyValue == parentId)
                            .Any(f => f.FolderId == d.FolderId))
                            .ProjectTo<DocDocumentListViewModel>()
                            .ToListAsync();

            var canViewPublicOnly = settings.IsRestrictPrivateDocAccessOn && await _docService.IsUserRestrictedFromPrivateDocuments();
            if (canViewPublicOnly)
            {
                docs = docs.Where(d => (d.FolderIsPublic || d.FolderCreatedBy == _user.GetUserName()) && (!d.IsPrivate || d.CreatedBy == _user.GetUserName())).ToList();
            }
            return docs;
        }

        public async Task<List<DocDocumentListViewModel>> CreateViewModelForDownload(int parentId, string selection)
        {
            var keys = selection.Split(',');
            var selectionList = keys.Select(k => new DocDocumentListViewModel { DocId = Convert.ToInt32(k) });

            var docs = await _docService.DocDocuments.Where(d => _docService.DocFolders.Where(f => f.SystemType == SystemTypeCode.Patent && ((f.DataKey == "InvId" && f.DataKeyValue == parentId)))
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
