using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatSearchNotify
    {
        [Key]
        public int NotifyId { get; set; }

        public int CritDtlId { get; set; }
        public string? EmailsToNotify { get; set; }
        public string? EmailsToNotify_Cc { get; set; }

        public int QESetupId { get; set; }
        public QEMain? QEMain { get; set; }
        public SearchCriteriaDetail? SearchCriteriaDetail { get; set; }
    }
}
