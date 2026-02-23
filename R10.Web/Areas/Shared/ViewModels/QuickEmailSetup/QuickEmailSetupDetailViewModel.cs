using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickEmailSetupDetailViewModel: QEMain 
    {
        public string? ScreenName { get; set; }

        public string? SystemType { get; set; }

        public string? DataSourceName { get; set; }

        public string? LanguageName { get; set; }

        public bool FromOption { get; set; }

        public bool ReplyOption { get; set; }

        public int OldQESetupID { get; set; }

        public bool CopyRecipients { get; set; }

        public bool? eSignature { get; set; }

        public List<string>? Tags { get; set; }
        [Display(Name = "Category")]
        public string? QECat { get; set; }
    }
}
