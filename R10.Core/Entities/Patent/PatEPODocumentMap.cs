using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatEPODocumentMap : BaseEntity
    {
        [Key]
        public int MapId { get; set; }

        [Display(Name = "Communication Code")]
        [StringLength(25)]
        public string? DocumentCode { get; set; }


        [Display(Name = "Communication Name")]
        [StringLength(255)]
        public string? DocumentName { get; set; }

        [Display(Name = "Download?")]
        public bool Enabled { get; set; } = true;

        public string? Language { get; set; }

        [Display(Name = "Critical?")]
        public bool IsCritical { get; set; } = true;
                
        [Display(Name = "Docket Required?")]
        public bool IsActRequired { get; set; }

        [Display(Name = "Check Docket?")]
        public bool CheckAct { get; set; }

        [Display(Name = "Forward document to client?")]
        public bool SendToClient { get; set; }

        [Display(Name = "Auto-Generate Action?")]
        public bool IsGenAction { get; set; } = true;
    }
}
