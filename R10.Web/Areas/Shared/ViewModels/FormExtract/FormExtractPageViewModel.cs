using R10.Core.DTOs;

namespace R10.Web.Models.PageViewModels
{
    public class FormExtractPageViewModel
    {

        public List<LookupDTO>? FRSystemList { get; set; }

        public string? DefaultSystemType { get; set; }
        public string? DefaultSourceCode { get; set; }
        public string? DefaultFormType{ get; set; }
        public string? DefaultFormName{ get; set; }
        public string? DefaultDocDesc { get; set; }
    }
}
