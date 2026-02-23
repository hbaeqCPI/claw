using System.ComponentModel.DataAnnotations;
using R10.Core.Entities;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LetterMainDetailViewModel : LetterMainDetail
    {
        [Display(Name = "Screen")]

        public string? ScreenName { get; set; }


        [Display(Name = "Category")]
        public string? LetCatDesc { get; set; }

        [Display(Name = "Sub Category")]
        public string? LetSubCat { get; set; }

        public string? SystemType { get; set; }
        public int CopySourceLetId { get; set; }

        [Display(Name = "eSignature Email Template")]
        public string? SignatureQESetupName { get; set; }
        public List<string>? Tags { get; set; }
    }
}
