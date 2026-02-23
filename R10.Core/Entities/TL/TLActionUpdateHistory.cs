using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TLActionUpdateHistory
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

        [Display(Name = "Trademark Office Action")]
        public string? SearchAction { get; set; }

        [Display(Name = "Your Action")]
        public string? TMSAction { get; set; }

        [Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

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
