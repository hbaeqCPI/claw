using R10.Web.Areas.Shared.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface ITmcImageViewModelService : IImageViewModelService
    {
        Task<List<DocDocumentListViewModel>> CreateViewModelForDownload(int parentId, string selection);
    }
}
