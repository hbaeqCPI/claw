using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TLTmkNameUpdateHistory
    {
        public int TLTmkId { get; set; }
        
        public int TMSTmkId { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name="Changed On")]
        public DateTime ChangeDate { get; set; }

        [Display(Name = "Trademark Office Trademark Name")]
        public string? PTOTrademarkName { get; set; }

        [Display(Name = "Your Trademark Name")]
        public string? TMSTrademarkName { get; set; }

        [Display(Name = "Source")]
        public string? UpdateSource { get; set; }

        [Display(Name = "By")]
        public string? ChangedBy { get; set; }

        [Display(Name = "Reverted On")]
        public DateTime? UndoDate { get; set; }

        [Display(Name = "Reverted By")]
        public string? UndoBy { get; set; }

        public int Reverted { get; set; }
        public int JobId { get; set; }
        public int LogId { get; set; }
    }
}
