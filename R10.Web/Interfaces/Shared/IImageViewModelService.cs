using R10.Web.Areas.Shared.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IImageViewModelService
    {

        //Task<ImageViewModel> CreateViewModelForDetailScreen(int parentId, int imageId);
        Task<List<DocDocumentListViewModel>> CreateViewModelForList(int parentId);
        //Task<string> IsLocked(int fileId, string userName);
        //Task CheckoutImage(int fileId, string userName);
    }
}
