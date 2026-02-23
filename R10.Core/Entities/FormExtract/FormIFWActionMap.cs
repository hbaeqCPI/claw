using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.FormExtract
{
    public class FormIFWActionMap : BaseEntity
    {
        [Key]
        public int MapId { get; set; }

        public int DocTypeId { get; set; }

        [StringLength(60)]
        [Display(Name = "Action Type")]
        [Required]
        public string PMSActionType{ get; set; }

        [StringLength(60)]
        [Display(Name = "Action Due")]
        [Required]
        public string PMSActionDue { get; set; }

        [Display(Name = "Yr")]
        public int Yr { get; set; } = 0;

        [Display(Name = "Mo")]
        public int Mo { get; set; } = 0;

        [Display(Name = "Dy")]
        public int Dy { get; set; } = 0;

        [StringLength(20)]
        [Display(Name = "Indicator")]
        [Required]
        public string Indicator { get; set; }

        [NotMapped]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }
    }
}
