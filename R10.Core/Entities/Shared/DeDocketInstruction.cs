using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{

    public class DeDocketInstruction : BaseEntity
    {
        [Key]
        public int InstructionId { get; set; }

        [StringLength(45)]
        [Required(ErrorMessage = "Instruction is required.")]
        [Display(Name = "Instruction")]
        public string?  Instruction { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string?  Description { get; set; }
        

        [Display(Name = "In Use?")]
        public bool InUse { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "Close Deadline With")]
        public string? CloseDeadlineWith { get; set; }

        [Display(Name = "Document Required?")]
        public bool DocumentRequired { get; set; }

        [Display(Name = "Patent")]
        public bool Patent { get; set; }

        [Display(Name = "Trademark")]
        public bool Trademark { get; set; }

        [Display(Name = "General Matter")]
        public bool GeneralMatter { get; set; }
        
        [Display(Name = "Indicators")]
        public string? Indicators { get; set; }

        [NotMapped]
        public string[]? IndicatorList { get; set; }
    }

}
