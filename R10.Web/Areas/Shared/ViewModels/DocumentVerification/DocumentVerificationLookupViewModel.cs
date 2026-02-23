using R10.Web.Helpers;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocumentVerificationLookupViewModel
    {
        public string? Id { get; set; }
        public string? ActionType { get; set; }
        public string? BaseDate { get; set; }
    }

    public class DocVerificationActionTypeListViewModel
    {
        public string? SystemType { get; set; }
        public int? ActId { get; set; }
        public int? ActionTypeId { get; set; }
    }
}
