using System.ComponentModel.DataAnnotations;

namespace R10.Web.Models.MailViewModels
{
    public class MailDownloadRuleViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int ActionId { get; set; }

        public bool Enabled { get; set; } = true;

        public bool DownloadAttachments { get; set; }

        public int OrderOfEntry { get; set; }

        public bool StopProcessing { get; set; } = true;

        public bool DoNotMove { get; set; }
        public string? DownloadFolderId { get; set; }

        public string? tStamp { get; set; }

        public List<MailDownloadRuleConditionViewModel>? RuleConditions { get; set; }

        public List<string>? Responsibles { get; set; }
    }
}
