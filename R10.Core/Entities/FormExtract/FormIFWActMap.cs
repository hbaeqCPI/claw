using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace R10.Core.Entities.FormExtract
{
    public class FormIFWActMap : BaseEntity
    {
        [Key]
        public int MapHdrId { get; set; }

        public int DocTypeId { get; set; }

        [Display(Name = "Auto-Generate Action?")]
        public bool IsGenAction { get; set; } = true;

        [Display(Name = "Compare Action?")]
        public bool IsCompare { get; set; } = true;

        public FormIFWDocType? FormIFWDocType { get; set; }

        public List<FormIFWActMapPat>? FormIFWActMapPats { get; set; }
        public List<FormIFWActMapTmk>? FormIFWActMapTmks { get; set; }
    }
}
