using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Interfaces
{
    public interface IPatImageInvViewModelService : IImageViewModelService
    {
        Task<List<DocDocumentListViewModel>> CreateViewModelForDownload(int parentId, string selection);
    }
}
