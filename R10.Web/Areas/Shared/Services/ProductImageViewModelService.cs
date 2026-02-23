using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Helpers;
using System.Security.Claims;
using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Areas.Shared.Services
{
    public class ProductImageViewModelService : IProductImageViewModelService
    {
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly IDocumentService _docService;
        private readonly ClaimsPrincipal _user;

        public ProductImageViewModelService(
            ISystemSettings<DefaultSetting> settings,
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
            var docs = await _docService.DocDocuments.Where(d => _docService.DocFolders.Where(f => f.SystemType == SystemTypeCode.Shared && f.DataKey == "ProductId" && f.DataKeyValue == parentId)
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

    }
}
