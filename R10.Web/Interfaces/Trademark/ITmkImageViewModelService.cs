using System.Collections.Generic;
using System.Threading.Tasks;
using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Interfaces
{
    public interface ITmkImageViewModelService : IImageViewModelService
    {
        Task<List<DocDocumentListViewModel>> CreateViewModelForDownload(int parentId, string selection);
    }
}
