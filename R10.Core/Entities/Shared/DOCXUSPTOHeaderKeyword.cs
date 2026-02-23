using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;


namespace R10.Core.Entities
{
    public class DOCXUSPTOHeaderKeyword : BaseEntity
    {
        [Key]
        public int KId { get; set; }
        public int HId { get; set; }
        public string? KeywordName { get; set; }
        //public DOCXUSPTOHeader? Header { get; set; }

    }
}
