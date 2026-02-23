using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TLBiblioUpdateHistory
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

        [Display(Name = "Field Name")]
        public string? ChangeFieldDesc { get; set; }

        [Display(Name = "Old Value")]
        public string? OldValue { get; set; }

        [Display(Name = "New Value")]
        public string? NewValue { get; set; }
        
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
