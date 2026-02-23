using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocVerificationReviewFilterViewModel : DocVerificationReviewFilters
    {
        public string[]? Countries { get; set; }        
        public string[]? CaseTypes { get; set; }
        public string[]? Clients { get; set; }
    }
}
