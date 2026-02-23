using R10.Core.DTOs;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickEmailImageLinkViewModel: QEImagesLinksDTO
    {
        public string? IconClass { get; set; }
        
        public string? SharePointDocLibrary { get; set; } //sharepoint
        public string? ItemId { get; set; } 

    }
}
