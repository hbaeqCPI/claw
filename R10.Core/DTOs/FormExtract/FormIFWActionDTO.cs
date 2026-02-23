using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.DTOs
{
    [Keyless]
    public class FormIFWActionDueDTO
    {
        [Display(Name = "Description")]
        public string? ActionDesc { get; set; }

        [Display(Name = "Mail Room Date")]
        public DateTime? BaseDate { get; set; }

        [Display(Name = "Months to Reply")]
        public int? TermMonth { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

    }

    [Keyless]
    public class FormIFWActionRemarksDTO
    {
        public int  UsageId { get; set; }
        [Display(Name = "Field Name")]
        public string? FieldName{ get; set; }
        [Display(Name = "Field Label")]
        public string?  FieldLabel { get; set; }
        [Display(Name = "Data")]
        public string? FieldData { get; set; }
        public int EntryOrder { get; set; }

        [Display(Name = "Confidence")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double Confidence { get; set; }

        [NotMapped]
        public string? LabelAndData { get; set; }
    }

    [Keyless]
    public class FormIFWActionUpdateDTO
    {
        public bool Error { get; set; }
        public int ExistingActions { get; set; }
        public int ActionsAdded { get; set; }

    }

   
}
